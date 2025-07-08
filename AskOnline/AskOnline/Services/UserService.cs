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

    public async Task<bool> DeleteUserAsync(int id, int currentUserId, bool isAdmin)
    {
        var user = await _context.Users
            .Include(u => u.Questions)
                .ThenInclude(q => q.Answers)
            .Include(u => u.Questions)
                .ThenInclude(q => q.QuestionTags)
            .Include(u => u.Answers)
                .ThenInclude(a => a.Ratings)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
            return false;

        if (!isAdmin && currentUserId != id)
            throw new UnauthorizedAccessException();

        if (user.Role == Roles.Admin && !isAdmin)
            throw new UnauthorizedAccessException();

        // Delete ratings created by the user
        var userRatings = await _context.AnswerRatings
            .Where(r => r.UserId == id)
            .ToListAsync();
        _context.AnswerRatings.RemoveRange(userRatings);

        // Delete answers to user's questions
        var questionIds = user.Questions.Select(q => q.QuestionId).ToList();
        var relatedAnswers = await _context.Answers
            .Where(a => questionIds.Contains(a.QuestionId))
            .Include(a => a.Ratings)
            .ToListAsync();

        var answerRatings = relatedAnswers.SelectMany(a => a.Ratings).ToList();
        _context.AnswerRatings.RemoveRange(answerRatings);
        _context.Answers.RemoveRange(relatedAnswers);

        // Delete user's own answers
        _context.Answers.RemoveRange(user.Answers);

        // Delete question-tag links
        var questionTags = user.Questions.SelectMany(q => q.QuestionTags).ToList();
        _context.QuestionTags.RemoveRange(questionTags);

        // Delete user's questions
        _context.Questions.RemoveRange(user.Questions);

        // Delete the user
        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
        return true;
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

