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
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        /// <summary>
        /// Creates or updates a rating for an answer.
        /// </summary>
        /// <param name="request">The rating request.</param>
        /// <returns>The created or updated rating.</returns>
        // POST: api/ratings
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RatingResponseDto>> PostRating(RatingRequestDto request)
        {
            var result = await _ratingService.CreateOrUpdateRatingAsync(request);

            if (result == null)
                return NotFound("Answer not found or user not authenticated.");

            return Ok(result);
        }


        /// <summary>
        /// Deletes a rating for an answer.
        /// </summary>
        /// <param name="answerId">The ID of the answer.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        // DELETE: api/ratings/answer/5
        [Authorize]
        [HttpDelete("answer/{answerId}")]
        public async Task<IActionResult> DeleteRating(int answerId)
        {
            var deleted = await _ratingService.DeleteRatingAsync(answerId);
            if (!deleted)
                return NotFound("Rating not found or user unauthorized");

            return NoContent();
        }


        /// <summary>
        /// Gets the score for an answer.
        /// </summary>
        /// <param name="answerId">The ID of the answer.</param>
        /// <returns>The score of the answer.</returns>
        // GET: api/ratings/answer/5
        [HttpGet("answer/{answerId}")]
        public async Task<ActionResult<AnswerScoreDto>> GetAnswerScore(int answerId)
        {
            var score = await _ratingService.GetAnswerScoreAsync(answerId);
            if (score == null)
                return NotFound("Answer not found");

            return Ok(score);
        }

    }
}