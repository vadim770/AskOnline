using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AskOnline.Dtos;
using AskOnline.Services;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerService _answerService;

        public AnswersController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        /// <summary>
        /// Gets all answers for a specific question.
        /// </summary>
        /// <param name="questionId">The ID of the question.</param>
        /// <returns>A list of answers for the question.</returns>
        // GET: api/Answers/by-question/3
        [HttpGet("by-question/{questionId}")]
        public async Task<ActionResult<IEnumerable<AnswerResponseDto>>> GetAnswersForQuestion(int questionId)
        {
            var answerDtos = await _answerService.GetAnswersForQuestion(questionId);
            return Ok(answerDtos);
        }

        /// <summary>
        /// Creates a new answer for a question.
        /// </summary>
        /// <param name="request">The answer creation request.</param>
        /// <returns>The created answer.</returns>
        // POST: api/Answers
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AnswerResponseDto>> PostAnswer([FromBody] AnswerRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _answerService.CreateAnswerAsync(request);
            if (response == null)
                return NotFound("Question not found.");

            return CreatedAtAction(nameof(GetAnswersForQuestion), new { questionId = request.QuestionId }, response);
        }


        /// <summary>
        /// Deletes an answer.
        /// </summary>
        /// <param name="id">The ID of the answer to delete.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            return await _answerService.DeleteAnswerAsync(id);
        }

        /// <summary>
        /// Updates an existing answer.
        /// </summary>
        /// <param name="id">The ID of the answer to update.</param>
        /// <param name="dto">The answer update data.</param>
        /// <returns>The updated answer.</returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnswer(int id, AnswerUpdateDto dto)
        {
            try
            {
                var updated = await _answerService.UpdateAnswerAsync(id, dto);
                if (updated == null)
                    return NotFound();

                return Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
