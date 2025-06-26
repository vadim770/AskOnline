using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AskOnline.Data;
using AskOnline.Models;
using Microsoft.AspNetCore.Authorization;
using AskOnline.Dtos;
using System.Security.Claims;


namespace AskOnline.Controllers
{
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            return await _context.Questions.Include(q => q.User).Include(q => q.Answers).ThenInclude(a => a.User).ToListAsync(); ;
        }

        // GET: api/questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
                return NotFound();

            return question;
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

            // Optional: ensure user exists in DB
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
                return Unauthorized("User not found in database");

            var question = new Question
            {
                Title = request.Title,
                Body = request.Body,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var response = new QuestionResponseDto
            {
                QuestionId = question.QuestionId,
                Title = question.Title,
                Body = question.Body,
                CreatedAt = question.CreatedAt,
                UserId = question.UserId
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

            return NoContent(); // 204
        }



    }
}
