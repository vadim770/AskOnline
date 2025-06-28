using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TagsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Tags
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] TagCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Tag name is required.");

            var exists = _context.Tags.Any(t => t.Name.ToLower() == dto.Name.ToLower());
            if (exists)
                return Conflict("Tag with that name already exists.");

            var tag = new Tag
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim()
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            var result = new TagDto
            {
                TagId = tag.TagId,
                Name = tag.Name,
                Description = tag.Description
            };

            return CreatedAtAction(nameof(GetTag), new { id = tag.TagId }, result);
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();

            return new TagDto
            {
                TagId = tag.TagId,
                Name = tag.Name,
                Description = tag.Description
            };
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
        {
            var tags = _context.Tags.Select(t => new TagDto
            {
                TagId = t.TagId,
                Name = t.Name,
                Description = t.Description
            });

            return Ok(await tags.ToListAsync());
        }

        // DELETE: api/tags/{id}
        [Authorize(Roles = Roles.Admin)] // Only admins can delete tags
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tags
                .Include(t => t.QuestionTags)
                .FirstOrDefaultAsync(t => t.TagId == id);

            if (tag == null)
                return NotFound("Tag not found.");

            // Remove the links to questions
            _context.QuestionTags.RemoveRange(tag.QuestionTags);

            // Remove the tag itself
            _context.Tags.Remove(tag);

            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }


    }
}
