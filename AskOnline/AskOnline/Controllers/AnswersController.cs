using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AskOnline.Data;
using AskOnline.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        public async Task<ActionResult<IEnumerable<Answer>>> GetAnswersForQuestion(int questionId)
        {
            var answers = await _context.Answers
                .Where(a => a.QuestionId == questionId)
                .Include(a => a.User)
                .ToListAsync();

            return Ok(answers);
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

            var response = new AnswerResponseDto
            {
                AnswerId = answer.AnswerId,
                Body = answer.Body,
                CreatedAt = answer.CreatedAt,
                QuestionId = answer.QuestionId,
                UserId = answer.UserId
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



    }
}
