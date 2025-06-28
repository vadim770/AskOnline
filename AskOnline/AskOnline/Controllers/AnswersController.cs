using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AskOnline.Data;
using AskOnline.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AskOnline.Dtos;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnswersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Answers/by-question/3
        [HttpGet("by-question/{questionId}")]
        public async Task<ActionResult<IEnumerable<AnswerResponseDto>>> GetAnswersForQuestion(int questionId)
        {
            var answers = await _context.Answers
                .Where(a => a.QuestionId == questionId)
                .Include(a => a.User)
                .ToListAsync();

            var isAdmin = User.Identity.IsAuthenticated && User.IsInRole(Roles.Admin);

            var answerDtos = answers.Select(a => new AnswerResponseDto
            {
                AnswerId = a.AnswerId,
                Body = a.Body,
                CreatedAt = a.CreatedAt,
                QuestionId = a.QuestionId,
                User = MapUserDto(a.User, isAdmin)
            }).ToList();

            return Ok(answerDtos);
        }

        // POST: api/Answers
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AnswerResponseDto>> PostAnswer(AnswerRequestDto request)
        {
            // find the question
            var question = await _context.Questions.FindAsync(request.QuestionId);
            if (question == null)
                return NotFound("Question not found.");

            // get the user ID from the JWT
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var answer = new Answer
            {
                Body = request.Body,
                CreatedAt = DateTime.UtcNow,
                QuestionId = request.QuestionId,
                UserId = userId
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            // Load the user information for the response
            var answerWithUser = await _context.Answers
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AnswerId == answer.AnswerId);

            var isAdmin = User.IsInRole(Roles.Admin);

            var response = new AnswerResponseDto
            {
                AnswerId = answerWithUser.AnswerId,
                Body = answerWithUser.Body,
                CreatedAt = answerWithUser.CreatedAt,
                QuestionId = answerWithUser.QuestionId,
                User = MapUserDto(answerWithUser.User, isAdmin)
            };

            return CreatedAtAction(nameof(GetAnswersForQuestion), new { questionId = answer.QuestionId }, response);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole(Roles.Admin);

            var answer = await _context.Answers.FindAsync(id);

            if (answer == null)
                return NotFound();

            if (!isAdmin && answer.UserId != userId)
                return Forbid();

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            return NoContent();
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
    }
}
