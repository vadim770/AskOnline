using AskOnline.Dtos;
using AskOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionRatingsController : ControllerBase
    {
        private readonly QuestionRatingService _ratingService;

        public QuestionRatingsController(QuestionRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        // POST: api/question-ratings
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuestionRatingResponseDto>> PostRating(QuestionRatingRequestDto request)
        {
            var result = await _ratingService.CreateOrUpdateRatingAsync(request);

            if (result == null)
                return NotFound("Question not found or user not authenticated.");

            return Ok(result);
        }

        // DELETE: api/question-ratings/question/5
        [Authorize]
        [HttpDelete("question/{questionId}")]
        public async Task<IActionResult> DeleteRating(int questionId)
        {
            var deleted = await _ratingService.DeleteRatingAsync(questionId);
            if (!deleted)
                return NotFound("Rating not found or user unauthorized");

            return NoContent();
        }

        // GET: api/question-ratings/question/5
        [HttpGet("question/{questionId}")]
        public async Task<ActionResult<QuestionScoreDto>> GetQuestionScore(int questionId)
        {
            var score = await _ratingService.GetQuestionScoreAsync(questionId);
            if (score == null)
                return NotFound("Question not found");

            return Ok(score);
        }
    }
}
