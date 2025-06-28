using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AskOnline.Data;
using AskOnline.Models;
using AskOnline.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isAdmin = User.IsInRole(Roles.Admin);

            var response = users.Select(user =>
            {
                var dto = new UserResponseDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = isAdmin ? user.Role : "User"
                };

                if (currentUserId == user.UserId.ToString() || isAdmin)
                {
                    dto.Email = user.Email;
                    dto.CreatedAt = user.CreatedAt;
                }

                return dto;
            });

            return Ok(response);
        }


        // GET: api/Users/1
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(Roles.Admin);

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role
            };

            if (currentUserId == user.UserId.ToString() || isAdmin)
            {
                // Show more info if user is owner or admin
                response.Email = user.Email;
                response.CreatedAt = user.CreatedAt;
            }

            return Ok(response);
        }



        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        // DELETE: api/Users/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = User.IsInRole(Roles.Admin);

            var user = await _context.Users
                .Include(u => u.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(u => u.Questions)
                    .ThenInclude(q => q.QuestionTags)
                .Include(u => u.Answers)
                    .ThenInclude(a => a.Ratings)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            if (!isAdmin && currentUserId != id)
                return Forbid();

            if (user.Role == Roles.Admin && !isAdmin)
                return Forbid(); // Prevent users from deleting admins

            // Delete ratings created by the user
            var userRatings = await _context.AnswerRatings
                .Where(r => r.UserId == id)
                .ToListAsync();
            _context.AnswerRatings.RemoveRange(userRatings);

            // Delete answers to user's questions
            var questionIds = user.Questions.Select(q => q.QuestionId).ToList();
            var relatedAnswers = await _context.Answers
                .Where(a => questionIds.Contains(a.QuestionId))
                .Include(a => a.Ratings)
                .ToListAsync();

            // Delete ratings on those answers
            var answerRatings = relatedAnswers.SelectMany(a => a.Ratings).ToList();
            _context.AnswerRatings.RemoveRange(answerRatings);

            // Delete answers to user's questions
            _context.Answers.RemoveRange(relatedAnswers);

            // Delete user's own answers
            _context.Answers.RemoveRange(user.Answers);

            // Delete question-tag links
            var questionTags = user.Questions.SelectMany(q => q.QuestionTags).ToList();
            _context.QuestionTags.RemoveRange(questionTags);

            // Delete user's questions
            _context.Questions.RemoveRange(user.Questions);

            // Finally, delete user
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            return NoContent();
        }



    }
}
