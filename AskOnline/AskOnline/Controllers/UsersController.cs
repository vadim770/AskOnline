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

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }



        // GET: api/Users/1
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var dto = await _userService.GetUserByIdAsync(id);
            return dto == null ? NotFound() : Ok(dto);
        }

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

        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
        {
            var userId = _userService.GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var dto = await _userService.GetUserByIdAsync(userId.Value);
            return dto == null ? NotFound() : Ok(dto);
        }

        // GET: api/users/1/questions
        [HttpGet("{id}/questions")]
        public async Task<ActionResult<IEnumerable<QuestionResponseDto>>> GetUserQuestions(int id)
        {
            var questions = await _questionService.GetQuestionsByUserIdAsync(id);
            return Ok(questions);
        }

        // GET: api/users/1/answers
        [HttpGet("{id}/answers")]
        public async Task<ActionResult<IEnumerable<AnswerResponseDto>>> GetUserAnswers(int id)
        {
            var answers = await _answerService.GetAnswersByUserIdAsync(id);
            return Ok(answers);
        }

        // GET: api/users/1/comments
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetUserComments(int id)
        {
            var comments = await _commentService.GetCommentsByUserIdAsync(id);
            return Ok(comments);
        }






    }
}
