using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AskOnline.Data;
using AskOnline.Models;
using Microsoft.AspNetCore.Authorization;
using AskOnline.Dtos;
using System.Security.Claims;
using AskOnline.Services;


namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly QuestionService _questionService;

        public QuestionsController(QuestionService questionService)
        {
            _questionService = questionService;
        }

        // GET: api/questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionResponseDto>>> GetQuestions()
        {
            var questionDtos = await _questionService.GetAllQuestionsAsync();
            return Ok(questionDtos);
        }




        // GET: api/questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionResponseDto>> GetQuestion(int id)
        {
            var result = await _questionService.GetQuestionByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }


        // POST: api/questions
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuestionResponseDto>> PostQuestion(QuestionRequestDto request)
        {
            var response = await _questionService.CreateQuestionAsync(request);
            if (response == null)
                return Unauthorized("User not found or unauthorized");

            return CreatedAtAction(nameof(GetQuestion), new { id = response.QuestionId }, response);
        }




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


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, QuestionUpdateDto dto)
        {
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



    }
}
