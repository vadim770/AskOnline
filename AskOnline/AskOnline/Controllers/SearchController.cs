using AskOnline.Dtos;
using AskOnline.Services;
using Microsoft.AspNetCore.Mvc;

namespace AskOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Search questions with various filters and sorting options
        /// </summary>
        /// <param name="q">Search query (can include [tagname] syntax for tag filtering)</param>
        /// <param name="sortBy">Sort order: Relevance, Newest, Active, Score</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 15, max: 50)</param>
        /// <param name="noAnswers">Filter questions with no answers</param>
        /// <param name="noUpvotedAnswers">Filter questions with no upvoted answers</param>
        /// <param name="olderThanDays">Filter questions older than specified days</param>
        /// <param name="tags">Additional tags to filter by (comma-separated)</param>
        /// <returns>Paginated search results</returns>
        [HttpGet]
        public async Task<ActionResult<SearchResultDto>> Search(
            [FromQuery] string? q,
            [FromQuery] SearchSortBy sortBy = SearchSortBy.Relevance,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] bool? noAnswers = null,
            [FromQuery] bool? noUpvotedAnswers = null,
            [FromQuery] int? olderThanDays = null,
            [FromQuery] string? tags = null)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 15;
                if (pageSize > 50) pageSize = 50; // Limit max page size

                // Parse comma-separated tags
                var tagList = string.IsNullOrWhiteSpace(tags)
                    ? null
                    : tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(t => t.Trim())
                           .Where(t => !string.IsNullOrWhiteSpace(t))
                           .ToList();

                var searchRequest = new SearchRequestDto
                {
                    Query = q,
                    SortBy = sortBy,
                    Page = page,
                    PageSize = pageSize,
                    Filters = new SearchFilters
                    {
                        NoAnswers = noAnswers,
                        NoUpvotedAnswers = noUpvotedAnswers,
                        OlderThanDays = olderThanDays,
                        Tags = tagList
                    }
                };

                var result = await _searchService.SearchQuestionsAsync(searchRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching questions with query: {Query}", q);
                return StatusCode(500, new
                {
                    error = "Search failed",
                    message = "An error occurred while processing your search request."
                });
            }
        }

        /// <summary>
        /// Advanced search endpoint with full request body support
        /// </summary>
        /// <param name="request">Complete search request with all parameters</param>
        /// <returns>Paginated search results</returns>
        [HttpPost("advanced")]
        public async Task<ActionResult<SearchResultDto>> AdvancedSearch([FromBody] SearchRequestDto request)
        {
            try
            {
                // Validate and sanitize request
                if (request.Page < 1) request.Page = 1;
                if (request.PageSize < 1) request.PageSize = 15;
                if (request.PageSize > 50) request.PageSize = 50;

                var result = await _searchService.SearchQuestionsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during advanced search");
                return StatusCode(500, new
                {
                    error = "Advanced search failed",
                    message = "An error occurred while processing your advanced search request."
                });
            }
        }

        /// <summary>
        /// Get available search options and metadata
        /// </summary>
        /// <returns>Search configuration and available options</returns>
        [HttpGet("options")]
        public ActionResult GetSearchOptions()
        {
            return Ok(new
            {
                sortOptions = Enum.GetValues<SearchSortBy>().Select(s => new
                {
                    value = s.ToString().ToLower(),
                    display = s.ToString()
                }).ToList(),
                maxPageSize = 50,
                defaultPageSize = 15,
                tagSearchSyntax = "[tagname]",
                supportedFilters = new[]
                {
                    "noAnswers",
                    "noUpvotedAnswers",
                    "olderThanDays",
                    "tags"
                }
            });
        }
    }
}