using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AskOnline.Data;
using Microsoft.EntityFrameworkCore;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RatingsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/ratings
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RatingResponseDto>> PostRating(RatingRequestDto request)
        {
            // Get user ID from token
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if answer exists
            var answerExists = await _context.Answers.AnyAsync(a => a.AnswerId == request.AnswerId);
            if (!answerExists)
                return NotFound("Answer not found");

            // Check if user already rated this answer
            var existingRating = await _context.AnswerRatings
                .FirstOrDefaultAsync(ar => ar.AnswerId == request.AnswerId && ar.UserId == userId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.IsUpvote = request.IsUpvote;
                existingRating.CreatedAt = DateTime.UtcNow; // Update timestamp
            }
            else
            {
                // Create new rating
                existingRating = new AnswerRating
                {
                    AnswerId = request.AnswerId,
                    UserId = userId,
                    IsUpvote = request.IsUpvote,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AnswerRatings.Add(existingRating);
            }

            await _context.SaveChangesAsync();

            var response = new RatingResponseDto
            {
                RatingId = existingRating.RatingId,
                AnswerId = existingRating.AnswerId,
                IsUpvote = existingRating.IsUpvote,
                CreatedAt = existingRating.CreatedAt
            };

            return Ok(response);
        }

        // DELETE: api/ratings/answer/5
        [Authorize]
        [HttpDelete("answer/{answerId}")]
        public async Task<IActionResult> DeleteRating(int answerId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var rating = await _context.AnswerRatings
                .FirstOrDefaultAsync(ar => ar.AnswerId == answerId && ar.UserId == userId);

            if (rating == null)
                return NotFound("Rating not found");

            _context.AnswerRatings.Remove(rating);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/ratings/answer/5
        [HttpGet("answer/{answerId}")]
        public async Task<ActionResult<AnswerScoreDto>> GetAnswerScore(int answerId)
        {
            var ratings = await _context.AnswerRatings
                .Where(ar => ar.AnswerId == answerId)
                .ToListAsync();

            var upvotes = ratings.Count(r => r.IsUpvote);
            var downvotes = ratings.Count(r => !r.IsUpvote);

            bool? userVote = null;
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var userRating = ratings.FirstOrDefault(r => r.UserId == userId);
                userVote = userRating?.IsUpvote;
            }

            var response = new AnswerScoreDto
            {
                AnswerId = answerId,
                UpvoteCount = upvotes,
                DownvoteCount = downvotes,
                TotalScore = upvotes - downvotes,
                UserVote = userVote
            };

            return Ok(response);
        }
    }
}