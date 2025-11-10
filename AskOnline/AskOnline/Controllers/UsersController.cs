using AskOnline.Dtos;
using AskOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService;
        private readonly ICommentService _commentService;

        public UsersController(IUserService userService,
                                      IQuestionService questionService, IAnswerService answerService, ICommentService commentService)
        {
            _userService = userService;
            _questionService = questionService;
            _answerService = answerService;
            _commentService = commentService;
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A list of all users.</returns>
        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }



        /// <summary>
        /// Gets a specific user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>The user with the specified ID.</returns>
        // GET: api/Users/1
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var dto = await _userService.GetUserByIdAsync(id);
            return dto == null ? NotFound() : Ok(dto);
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        // DELETE: api/Users/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            try
            {
                var success = await _userService.DeleteUserAsync(id, currentUserId.Value, isAdmin);
                return success ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Gets the currently authenticated user.
        /// </summary>
        /// <returns>The currently authenticated user.</returns>
        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
        {
            var userId = _userService.GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var dto = await _userService.GetUserByIdAsync(userId.Value);
            return dto == null ? NotFound() : Ok(dto);
        }

        /// <summary>
        /// Gets all questions asked by a specific user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of questions asked by the user.</returns>
        // GET: api/users/1/questions
        [HttpGet("{id}/questions")]
        public async Task<ActionResult<IEnumerable<QuestionResponseDto>>> GetUserQuestions(int id)
        {
            var questions = await _questionService.GetQuestionsByUserIdAsync(id);
            return Ok(questions);
        }

        /// <summary>
        /// Gets all answers provided by a specific user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of answers provided by the user.</returns>
        // GET: api/users/1/answers
        [HttpGet("{id}/answers")]
        public async Task<ActionResult<IEnumerable<AnswerResponseDto>>> GetUserAnswers(int id)
        {
            var answers = await _answerService.GetAnswersByUserIdAsync(id);
            return Ok(answers);
        }

        /// <summary>
        /// Gets all comments made by a specific user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of comments made by the user.</returns>
        // GET: api/users/1/comments
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetUserComments(int id)
        {
            var comments = await _commentService.GetCommentsByUserIdAsync(id);
            return Ok(comments);
        }






    }
}
