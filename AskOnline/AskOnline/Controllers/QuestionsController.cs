using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AskOnline.Dtos;
using AskOnline.Services;


namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Gets all questions.
        /// </summary>
        /// <returns>A list of all questions.</returns>
        // GET: api/questions                                                                                                                                                          
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionResponseDto>>> GetQuestions()
        {
            var questionDtos = await _questionService.GetAllQuestionsAsync();
            return Ok(questionDtos);
        }




        /// <summary>
        /// Gets a specific question by its ID.
        /// </summary>
        /// <param name="id">The ID of the question.</param>
        /// <returns>The question with the specified ID.</returns>
        // GET: api/questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionResponseDto>> GetQuestion(int id)
        {
            var result = await _questionService.GetQuestionByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }


        /// <summary>
        /// Creates a new question.
        /// </summary>
        /// <param name="request">The question creation request.</param>
        /// <returns>The created question.</returns>
        // POST: api/questions
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuestionResponseDto>> PostQuestion([FromBody] QuestionRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _questionService.CreateQuestionAsync(request);
            if (response == null)
                return Unauthorized("User not found or unauthorized");

            return CreatedAtAction(nameof(GetQuestion), new { id = response.QuestionId }, response);
        }





        /// <summary>
        /// Deletes a question.
        /// </summary>
        /// <param name="id">The ID of the question to delete.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                var deleted = await _questionService.DeleteQuestionAsync(id);
                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        /// <summary>
        /// Updates an existing question.
        /// </summary>
        /// <param name="id">The ID of the question to update.</param>
        /// <param name="dto">The question update data.</param>
        /// <returns>The updated question.</returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] QuestionUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _questionService.UpdateQuestionAsync(id, dto);
                if (updated == null)
                    return NotFound();

                return Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Gets a list of recent questions.
        /// </summary>
        /// <param name="limit">The maximum number of questions to return.</param>
        /// <returns>A list of recent questions.</returns>
        [HttpGet("recent")]
        public async Task<ActionResult<List<QuestionResponseDto>>> GetRecentQuestions([FromQuery] int limit = 20)
        {
            try
            {
                // Validate limit to prevent abuse
                if (limit <= 0) limit = 20;
                if (limit > 100) limit = 100; // Max 100 questions

                var questions = await _questionService.GetRecentQuestionsAsync(limit);
                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch recent questions" });
            }
        }


    }
}
