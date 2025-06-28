using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AskOnline.Data;
using AskOnline.Models;
using Microsoft.AspNetCore.Authorization;
using AskOnline.Dtos;
using System.Security.Claims;


namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuestionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionResponseDto>>> GetQuestions()
        {
            var questions = await _context.Questions
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Ratings)
                .ToListAsync();

            var isAdmin = User.Identity.IsAuthenticated && User.IsInRole(Roles.Admin);
            int? currentUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            var questionDtos = new List<QuestionResponseDto>();

            foreach (var q in questions)
            {
                var answerDtos = new List<AnswerResponseDto>();
                foreach (var a in q.Answers)
                {
                    var dto = await MapAnswerToDto(a, isAdmin, currentUserId);
                    answerDtos.Add(dto);
                }

                var questionDto = new QuestionResponseDto
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Body = q.Body,
                    CreatedAt = q.CreatedAt,
                    User = MapUserDto(q.User, isAdmin),
                    Answers = answerDtos,
                    Tags = q.QuestionTags.Select(qt => new TagDto
                    {
                        TagId = qt.Tag.TagId,
                        Name = qt.Tag.Name,
                        Description = qt.Tag.Description
                    }).ToList()
                };

                questionDtos.Add(questionDto);
            }

            return Ok(questionDtos);
        }



        // GET: api/questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionResponseDto>> GetQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
                return NotFound();

            var isAdmin = User.Identity.IsAuthenticated && User.IsInRole(Roles.Admin);

            var response = new QuestionResponseDto
            {
                QuestionId = question.QuestionId,
                Title = question.Title,
                Body = question.Body,
                CreatedAt = question.CreatedAt,
                User = MapUserDto(question.User, isAdmin),
                Answers = question.Answers.Select(a => new AnswerResponseDto
                {
                    AnswerId = a.AnswerId,
                    Body = a.Body,
                    CreatedAt = a.CreatedAt,
                    QuestionId = a.QuestionId,
                    User = MapUserDto(a.User, isAdmin)
                }).ToList(),
                Tags = question.QuestionTags.Select(qt => new TagDto
                {
                    TagId = qt.Tag.TagId,
                    Name = qt.Tag.Name,
                    Description = qt.Tag.Description
                }).ToList()
            };

            return response;
        }

        // POST: api/questions
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuestionResponseDto>> PostQuestion(QuestionRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return Unauthorized("Missing user ID in token");

            int userId = int.Parse(userIdString);

            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
                return Unauthorized("User not found in database");

            var question = new Question
            {
                Title = request.Title,
                Body = request.Body,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                QuestionTags = new List<QuestionTag>()
            };

            // Process each tag name in the request
            foreach (var tagName in request.TagNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                // Check if tag exists by name (case-insensitive)
                var tag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());

                if (tag == null)
                {
                    // Create new tag if not found
                    tag = new Tag
                    {
                        Name = tagName,
                        Description = "" // or set some default description or accept from request if you want
                    };

                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                // Attach the tag to the question via the join entity
                question.QuestionTags.Add(new QuestionTag
                {
                    Tag = tag,
                    Question = question
                });
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // Reload the question including user and tags for response
            var questionWithUser = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.QuestionId == question.QuestionId);

            var isAdmin = User.IsInRole(Roles.Admin);

            var response = new QuestionResponseDto
            {
                QuestionId = questionWithUser.QuestionId,
                Title = questionWithUser.Title,
                Body = questionWithUser.Body,
                CreatedAt = questionWithUser.CreatedAt,
                User = MapUserDto(questionWithUser.User, isAdmin),
                Answers = new List<AnswerResponseDto>(), // New question has no answers yet
                Tags = questionWithUser.QuestionTags
                    .Select(qt => new TagDto
                    {
                        TagId = qt.Tag.TagId,
                        Name = qt.Tag.Name,
                        Description = qt.Tag.Description
                    })
                    .ToList()
            };

            return CreatedAtAction(nameof(GetQuestion), new { id = question.QuestionId }, response);
        }



        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole(Roles.Admin);

            var question = await _context.Questions.FindAsync(id);

            if (question == null)
                return NotFound();

            if (!isAdmin && question.UserId != userId)
                return Forbid();

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            await CleanupUnusedTagsAsync();

            return NoContent(); // 204
        }

        private UserPublicDto MapUserDto(User user, bool isAdmin)
        {
            if (isAdmin)
            {
                return new UserAdminDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };
            }

            return new UserPublicDto
            {
                UserId = user.UserId,
                Username = user.Username
            };
        }

        private async Task CleanupUnusedTagsAsync()
        {
            var unusedTags = await _context.Tags
                .Where(tag => !tag.QuestionTags.Any())
                .ToListAsync();

            if (unusedTags.Any())
            {
                _context.Tags.RemoveRange(unusedTags);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<AnswerResponseDto> MapAnswerToDto(Answer answer, bool isAdmin, int? currentUserId)
        {
            var upvotes = answer.Ratings.Count(r => r.IsUpvote);
            var downvotes = answer.Ratings.Count(r => !r.IsUpvote);

            bool? currentUserVote = null;
            if (currentUserId.HasValue)
            {
                var userRating = answer.Ratings.FirstOrDefault(r => r.UserId == currentUserId.Value);
                currentUserVote = userRating?.IsUpvote;
            }

            return new AnswerResponseDto
            {
                AnswerId = answer.AnswerId,
                Body = answer.Body,
                CreatedAt = answer.CreatedAt,
                QuestionId = answer.QuestionId,
                User = MapUserDto(answer.User, isAdmin),
                UpvoteCount = upvotes,
                DownvoteCount = downvotes,
                TotalScore = upvotes - downvotes,
                CurrentUserVote = currentUserVote
            };
        }

    }
}
