using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AskOnline.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public async Task<string?> RegisterAsync(UserRegisterRequest request)
        {
            var existing = await _unitOfWork.Users.GetByEmailAsync(request.Email.ToLower());
            if (existing != null)
                return null;

            var existingUsername = await _unitOfWork.Users.GetByUsernameAsync(request.Username.Trim());
            if (existingUsername != null)
                return null;

            var role = request.Role == Roles.Admin || request.Role == Roles.User
                ? request.Role
                : Roles.User;

            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim(),
                CreatedAt = DateTime.UtcNow,
                Role = role,
                PasswordHash = _hasher.HashPassword(null!, request.Password)
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return role;
        }

        public async Task<string?> LoginAsync(LoginRequest login)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(login.Email.ToLower());
            if (user == null)
                return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);
            if (result != PasswordVerificationResult.Success)
                return null;

            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // create a security key from the JWT settings
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));
            // create signing credentials using the key and HmacSha256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // et token expiry time
            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:ExpiryMinutes"]!));

            // create the JWT token
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            // serialize the token to a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
