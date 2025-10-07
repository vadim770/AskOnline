using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AskOnline.Services
{
    public class SearchService : ISearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public SearchService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<SearchResultDto> SearchQuestionsAsync(SearchRequestDto request)
        {
            IQueryable<Question> query = _unitOfWork.Questions
                .Query()
                .Include(q => q.User)
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Ratings)
                .Include(q => q.Ratings);

            // Extract tags from query using [tagname] syntax
            var (searchText, extractedTags) = ExtractTagsFromQuery(request.Query);

            // Apply text search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = ApplyTextSearch(query, searchText);
            }

            // Apply tag filtering from [tagname] syntax
            if (extractedTags.Any())
            {
                query = ApplyTagFiltering(query, extractedTags);
            }

            // Apply additional tag filtering from filters
            if (request.Filters?.Tags != null && request.Filters.Tags.Any())
            {
                query = ApplyTagFiltering(query, request.Filters.Tags);
            }

            // Apply other filters
            if (request.Filters != null)
            {
                query = ApplyFilters(query, request.Filters);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, request.SortBy);

            // Apply pagination
            var questions = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Map to DTOs
            var questionDtos = questions.Select(MapToSearchDto).ToList();

            return new SearchResultDto
            {
                Questions = questionDtos,
                TotalCount = totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }

        private (string searchText, List<string> tags) ExtractTagsFromQuery(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return (string.Empty, new List<string>());

            // Regex to match [tagname] pattern
            var tagPattern = @"\[([^\[\]]+)\]";
            var matches = Regex.Matches(query, tagPattern);

            var extractedTags = matches
                .Cast<Match>()
                .Select(m => m.Groups[1].Value.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Remove tag patterns from search text
            var searchText = Regex.Replace(query, tagPattern, "").Trim();

            // Clean up multiple spaces
            searchText = Regex.Replace(searchText, @"\s+", " ");

            return (searchText, extractedTags);
        }

        private IQueryable<Question> ApplyTextSearch(IQueryable<Question> query, string searchText)
        {
            var searchTerm = searchText.ToLower();

            return query.Where(q =>
                EF.Functions.Like(q.Title.ToLower(), $"%{searchTerm}%") ||
                EF.Functions.Like(q.Body.ToLower(), $"%{searchTerm}%")
            );
        }

        private IQueryable<Question> ApplyTagFiltering(IQueryable<Question> query, List<string> tags)
        {
            foreach (var tag in tags)
            {
                var tagLower = tag.ToLower();
                query = query.Where(q =>
                    q.QuestionTags.Any(qt =>
                        EF.Functions.Like(qt.Tag.Name.ToLower(), tagLower)
                    )
                );
            }

            return query;
        }

        private IQueryable<Question> ApplyFilters(IQueryable<Question> query, SearchFilters filters)
        {
            // Filter questions with no answers
            if (filters.NoAnswers == true)
            {
                query = query.Where(q => !q.Answers.Any());
            }

            // Filter questions with no upvoted answers
            if (filters.NoUpvotedAnswers == true)
            {
                query = query.Where(q =>
                    !q.Answers.Any(a =>
                        a.Ratings.Any(r => r.IsUpvote)
                    )
                );
            }

            // Filter questions older than X days
            if (filters.OlderThanDays.HasValue && filters.OlderThanDays.Value > 0)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-filters.OlderThanDays.Value);
                query = query.Where(q => q.CreatedAt < cutoffDate);
            }

            return query;
        }

        private IQueryable<Question> ApplySorting(IQueryable<Question> query, SearchSortBy sortBy)
        {
            return sortBy switch
            {
                SearchSortBy.Newest => query.OrderByDescending(q => q.CreatedAt),

                SearchSortBy.Score => query.OrderByDescending(q =>
                    q.Ratings.Count(r => r.IsUpvote) - q.Ratings.Count(r => !r.IsUpvote)
                ),

                SearchSortBy.Active => query.OrderByDescending(q =>
                    q.Answers.Any()
                        ? q.Answers.Max(a => a.CreatedAt)
                        : q.CreatedAt
                ),

                SearchSortBy.Relevance or _ => query.OrderByDescending(q => q.CreatedAt) // Default to newest for now
            };
        }

        private QuestionSearchDto MapToSearchDto(Question question)
        {
            var answerCount = question.Answers?.Count ?? 0;
            var score = (question.Ratings?.Count(r => r.IsUpvote) ?? 0) -
                       (question.Ratings?.Count(r => !r.IsUpvote) ?? 0);

            var lastActivity = question.Answers?.Any() == true
                ? question.Answers.Max(a => a.CreatedAt)
                : question.CreatedAt;

            var hasUpvotedAnswers = question.Answers?.Any(a =>
                a.Ratings?.Any(r => r.IsUpvote) == true
            ) ?? false;

            return new QuestionSearchDto
            {
                QuestionId = question.QuestionId,
                Title = question.Title,
                Body = TruncateText(question.Body, 200), // Truncate for search results
                CreatedAt = question.CreatedAt,
                User = _userService.MapUserDto(question.User),
                Tags = question.QuestionTags?.Select(qt => new TagDto
                {
                    TagId = qt.Tag.TagId,
                    Name = qt.Tag.Name
                }).ToList() ?? new List<TagDto>(),
                AnswerCount = answerCount,
                Score = score,
                LastActivity = lastActivity,
                HasUpvotedAnswers = hasUpvotedAnswers
            };
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength).Trim() + "...";
        }

        private IQueryable<Question> ApplyRelevanceScoring(IQueryable<Question> query, string searchText)
        {
            var searchTerm = searchText.ToLower();

            return query.OrderByDescending(q =>
                EF.Functions.Like(q.Title.ToLower(), $"%{searchTerm}%") ? 2 :
                EF.Functions.Like(q.Body.ToLower(), $"%{searchTerm}%") ? 1 : 0
            ).ThenByDescending(q => q.CreatedAt);
        }
    }
}