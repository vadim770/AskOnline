using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AskOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SearchController> _logger;

        public SearchController(AppDbContext context, ILogger<SearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionResponseDto>>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<QuestionResponseDto>());
            }

            try
            {
                var searchTerm = q.Trim();

                var questions = await _context.Questions
                    .Include(qu => qu.User)
                    .Include(qu => qu.Answers)
                        .ThenInclude(a => a.User)
                    .Include(qu => qu.QuestionTags)
                        .ThenInclude(qt => qt.Tag)
                    .Where(qu =>
                        EF.Functions.Like(qu.Title.ToLower(), $"%{searchTerm.ToLower()}%") ||
                        EF.Functions.Like(qu.Body.ToLower(), $"%{searchTerm.ToLower()}%") ||
                        qu.QuestionTags.Any(qt => EF.Functions.Like(qt.Tag.Name.ToLower(), $"%{searchTerm.ToLower()}%"))
                    )

                    .OrderByDescending(qu => qu.CreatedAt)
                    .Take(50)
                    .ToListAsync();

                var questionDtos = questions.Select(q => new QuestionResponseDto
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Body = q.Body,
                    CreatedAt = q.CreatedAt,
                    User = new UserResponseDto
                    {
                        UserId = q.User.UserId,
                        Username = q.User.Username,
                        Email = null, // Hide email for privacy in search results
                        CreatedAt = q.User.CreatedAt,
                        Role = q.User.Role
                    },
                    Tags = q.QuestionTags.Select(qt => new TagDto
                    {
                        TagId = qt.Tag.TagId,
                        Name = qt.Tag.Name
                    }).ToList(),
                    Answers = q.Answers?.Select(a => new AnswerResponseDto
                    {
                        AnswerId = a.AnswerId
                    }).ToList() ?? new List<AnswerResponseDto>()
                }).ToList();

                return Ok(questionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching questions");
                return StatusCode(500, new { error = "Search failed" });
            }
        }
    }


}
