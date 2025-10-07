
using AskOnline.Dtos;
using AskOnline.Models;

namespace AskOnline.Services
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(int id, int currentUserId, bool isAdmin);
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        bool IsAuthenticated();
        bool IsCurrentUserAdmin();
        int? GetCurrentUserId();
        UserResponseDto MapUserDto(User user);
    }
}
