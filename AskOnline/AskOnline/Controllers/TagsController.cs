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
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Adds a tag to a question.
        /// </summary>
        /// <param name="request">The request containing the question ID and tag name.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
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


        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="dto">The tag creation data.</param>
        /// <returns>The created tag.</returns>
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


        /// <summary>
        /// Gets a specific tag by its ID.
        /// </summary>
        /// <param name="id">The ID of the tag.</param>
        /// <returns>The tag with the specified ID.</returns>
        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTag(int id)
        {
            var tagDto = await _tagService.GetTagByIdAsync(id);
            if (tagDto == null)
                return NotFound();

            return Ok(tagDto);
        }

        /// <summary>
        /// Gets all tags.
        /// </summary>
        /// <returns>A list of all tags.</returns>
        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
        {
            var tagDtos = await _tagService.GetAllTagsAsync();
            return Ok(tagDtos);
        }


        /// <summary>
        /// Deletes a tag.
        /// </summary>
        /// <param name="id">The ID of the tag to delete.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        // DELETE: api/tags/{id}
        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var success = await _tagService.DeleteTagAsync(id);
            if (!success)
                return NotFound("Tag not found.");

            return NoContent(); // 204
        }

        /// <summary>
        /// Removes a tag from a question.
        /// </summary>
        /// <param name="questionId">The ID of the question.</param>
        /// <param name="tagId">The ID of the tag.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
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
