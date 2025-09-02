using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using AskOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;
        private readonly QuestionService _questionService;
        private readonly AnswerService _answerService;

        public UsersController(AppDbContext context, UserService userService,
                                      QuestionService questionService, AnswerService answerService)
        {
            _context = context;
            _userService = userService;
            _questionService = questionService;
            _answerService = answerService;
        }

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }



        // GET: api/Users/1
        [Authorize]
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

        [Authorize]
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







    }
}
