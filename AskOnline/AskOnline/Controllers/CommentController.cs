using AskOnline.Dtos;
using AskOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// Gets all comments for a specific answer.
        /// </summary>
        /// <param name="answerId">The ID of the answer.</param>
        /// <returns>A list of comments for the answer.</returns>
        [HttpGet("answers/{answerId}/comments")]
        public async Task<IActionResult> GetCommentsForAnswer(int answerId)
        {
            var comments = await _commentService.GetCommentsByAnswerAsync(answerId);
            return Ok(comments);
        }

        /// <summary>
        /// Adds a new comment to an answer.
        /// </summary>
        /// <param name="answerId">The ID of the answer to add a comment to.</param>
        /// <param name="dto">The comment creation data.</param>
        /// <returns>The created comment.</returns>
        [Authorize]
        [HttpPost("answers/{answerId}/comments")]
        public async Task<IActionResult> AddCommentToAnswer(int answerId, CommentCreateDto dto)
        {
            var newComment = await _commentService.AddCommentToAnswerAsync(answerId, dto);
            return CreatedAtAction(nameof(GetCommentsForAnswer), new { answerId }, newComment);
        }

        /// <summary>
        /// Updates an existing comment.
        /// </summary>
        /// <param name="commentId">The ID of the comment to update.</param>
        /// <param name="dto">The comment update data.</param>
        /// <returns>The updated comment.</returns>
        [Authorize]
        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, CommentUpdateDto dto)
        {
            try
            {
                var updatedComment = await _commentService.UpdateCommentAsync(commentId, dto);
                if (updatedComment == null)
                    return NotFound();

                return Ok(updatedComment);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Deletes a comment.
        /// </summary>
        /// <param name="id">The ID of the comment to delete.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var result = await _commentService.DeleteCommentAsync(id);
                if (!result)
                    return NotFound();

                return NoContent(); // 204 = success but no content to return
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

    }
}
