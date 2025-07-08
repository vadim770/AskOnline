using AskOnline.Dtos;

namespace AskOnline.Services
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(UserRegisterRequest request);
        Task<string?> LoginAsync(LoginRequest request);
    }

}
