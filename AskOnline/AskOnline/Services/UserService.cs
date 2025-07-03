using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();

        var users = await _context.Users.ToListAsync();

        return users.Select(user =>
        {
            var dto = MapUserDto(user);

            return dto;
        }).ToList();
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return null;

        var isAdmin = IsCurrentUserAdmin();
        var currentUserId = GetCurrentUserId();

        var response = MapUserDto(user);

        return response;
    }


    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public bool IsCurrentUserAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user != null && user.IsInRole(Roles.Admin);
    }

    public int? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
    }

    public UserResponseDto MapUserDto(User user)
    {
        var isAdmin = IsCurrentUserAdmin();
        var currentUserId = GetCurrentUserId();

        var response = new UserResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = isAdmin ? user.Role : "User",
            CreatedAt = user.CreatedAt
        };

        if (currentUserId == user.UserId || isAdmin)
        {
            response.Email = user.Email;
        }

        return response;
    }
}

