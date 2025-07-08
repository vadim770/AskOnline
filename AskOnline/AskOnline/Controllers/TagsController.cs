using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using AskOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly TagService _tagService;

        public TagsController(TagService tagService)
        {
            _tagService = tagService;
        }

        [Authorize]
        [HttpPost("add-to-question")]
        public async Task<IActionResult> AddTagToQuestion(AddTagToQuestionRequestDto request)
        {
            var result = await _tagService.AddTagToQuestionAsync(request.QuestionId, request.TagName);

            return result switch
            {
                null => Ok("Tag added successfully."),
                "Question not found." => NotFound(result),
                "Forbidden" => Forbid(),
                "Tag already associated with question." => BadRequest(result),
                _ => BadRequest("Unexpected error.")
            };
        }


        // POST: api/Tags
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] TagCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Tag name is required.");

            var result = await _tagService.CreateTagAsync(dto.Name.Trim());
            if (result == null)
                return Conflict("Tag with that name already exists.");

            return CreatedAtAction(nameof(GetTag), new { id = result.TagId }, result);
        }


        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTag(int id)
        {
            var tagDto = await _tagService.GetTagByIdAsync(id);
            if (tagDto == null)
                return NotFound();

            return Ok(tagDto);
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
        {
            var tagDtos = await _tagService.GetAllTagsAsync();
            return Ok(tagDtos);
        }


        // DELETE: api/tags/{id}
        [Authorize(Roles = Roles.Admin)] // Only admins can delete tags
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var success = await _tagService.DeleteTagAsync(id);
            if (!success)
                return NotFound("Tag not found.");

            return NoContent(); // 204
        }

        // DELETE: api/Tags/remove-from-question
        [Authorize]
        [HttpDelete("remove-from-question")]
        public async Task<IActionResult> RemoveTagFromQuestion(int questionId, int tagId)
        {
            var success = await _tagService.RemoveTagFromQuestionAsync(questionId, tagId);
            if (!success)
                return NotFound("Can't remove");

            return NoContent(); // 204
        }


    }
}
