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

        // GET: api/Answers/by-question/3
        [HttpGet("by-question/{questionId}")]
        public async Task<ActionResult<IEnumerable<AnswerResponseDto>>> GetAnswersForQuestion(int questionId)
        {
            var answerDtos = await _answerService.GetAnswersForQuestion(questionId);
            return Ok(answerDtos);
        }

        // POST: api/Answers
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AnswerResponseDto>> PostAnswer(AnswerRequestDto request)
        {
            var response = await _answerService.CreateAnswerAsync(request);
            if (response == null)
                return NotFound("Question not found.");

            return CreatedAtAction(nameof(GetAnswersForQuestion), new { questionId = request.QuestionId }, response);
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            return await _answerService.DeleteAnswerAsync(id);
        }

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
