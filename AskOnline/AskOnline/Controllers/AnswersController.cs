using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AskOnline.Data;
using AskOnline.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AskOnline.Dtos;
using AskOnline.Services;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly AnswerService _answerService;
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public AnswersController(AppDbContext context, UserService userService, AnswerService answerService)
        {
            _context = context;
            _userService = userService;
            _answerService = answerService;
        }

        // GET: api/Answers/by-question/3
        [HttpGet("by-question/{questionId}")]
        public async Task<ActionResult<IEnumerable<AnswerResponseDto>>> GetAnswersForQuestion(int questionId)
        {
            var answerDtos = await _answerService.GetAnswersForQuestion(questionId, _userService.IsCurrentUserAdmin(),_userService.GetCurrentUserId());
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
                User = _userService.MapUserDto(answerWithUser.User, isAdmin)
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
