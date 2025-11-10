using Microsoft.AspNetCore.Mvc;
using AskOnline.Services;
using AskOnline.Dtos;

namespace AskOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">User registration data.</param>
        /// <returns>A confirmation message.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await _authService.RegisterAsync(request);
            if (role == null)
                return Conflict("User already exists.");

            return Ok(new { message = $"User registered as {role}" });
        }

        /// <summary>
        /// Authenticates a user and returns an authentication token.
        /// </summary>
        /// <param name="login">User login credentials.</param>
        /// <returns>An authentication token.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authService.LoginAsync(login);
            if (token == null)
                return Unauthorized("Invalid email or password.");

            return Ok(new { token });
        }
    }
}
