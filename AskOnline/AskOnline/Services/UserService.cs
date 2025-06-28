using System.Security.Claims;
using AskOnline.Dtos;
using AskOnline.Models;

public class UserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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

    public UserPublicDto MapUserDto(User user, bool isAdmin)
    {
        if (isAdmin)
        {
            return new UserAdminDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        return new UserPublicDto
        {
            UserId = user.UserId,
            Username = user.Username
        };
    }
}

