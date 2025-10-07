using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using System.Security.Claims;
using AskOnline.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();

        var users = await _unitOfWork.Users.GetAllAsync();

        return users.Select(user =>
        {
            var dto = MapUserDto(user);
            return dto;
        }).ToList();
    }

    public async Task<bool> DeleteUserAsync(int id, int currentUserId, bool isAdmin)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return false;

        if (!isAdmin && currentUserId != id)
            throw new UnauthorizedAccessException();

        if (user.Role == Roles.Admin && !isAdmin)
            throw new UnauthorizedAccessException();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Delete ratings created by the user
            var userAnswerRatings = await _unitOfWork.AnswerRatings.GetByUserIdAsync(id);
            foreach (var rating in userAnswerRatings)
            {
                await _unitOfWork.AnswerRatings.DeleteAsync(rating.RatingId);
            }

            var userQuestionRatings = await _unitOfWork.QuestionRatings.GetByUserIdAsync(id);
            foreach (var rating in userQuestionRatings)
            {
                await _unitOfWork.QuestionRatings.DeleteAsync(rating.RatingId);
            }

            // Delete comments by the user
            var userComments = await _unitOfWork.Comments.GetByUserIdAsync(id);
            foreach (var comment in userComments)
            {
                await _unitOfWork.Comments.DeleteAsync(comment.CommentId);
            }

            // Get user's questions
            var userQuestions = await _unitOfWork.Questions.GetByUserIdAsync(id);
            var questionIds = userQuestions.Select(q => q.QuestionId).ToList();

            // Delete answers to user's questions (and their ratings/comments)
            foreach (var questionId in questionIds)
            {
                var answers = await _unitOfWork.Answers.GetByQuestionIdAsync(questionId);
                foreach (var answer in answers)
                {
                    // Delete ratings for this answer
                    var answerRatings = await _unitOfWork.AnswerRatings.GetByAnswerIdAsync(answer.AnswerId);
                    foreach (var rating in answerRatings)
                    {
                        await _unitOfWork.AnswerRatings.DeleteAsync(rating.RatingId);
                    }

                    // Delete comments for this answer
                    var comments = await _unitOfWork.Comments.GetByAnswerIdAsync(answer.AnswerId);
                    foreach (var comment in comments)
                    {
                        await _unitOfWork.Comments.DeleteAsync(comment.CommentId);
                    }

                    // Delete the answer
                    await _unitOfWork.Answers.DeleteAsync(answer.AnswerId);
                }
            }

            // Delete user's own answers
            var userAnswers = await _unitOfWork.Answers.GetByUserIdAsync(id);
            foreach (var answer in userAnswers)
            {
                // Delete ratings for this answer
                var answerRatings = await _unitOfWork.AnswerRatings.GetByAnswerIdAsync(answer.AnswerId);
                foreach (var rating in answerRatings)
                {
                    await _unitOfWork.AnswerRatings.DeleteAsync(rating.RatingId);
                }

                // Delete comments for this answer
                var comments = await _unitOfWork.Comments.GetByAnswerIdAsync(answer.AnswerId);
                foreach (var comment in comments)
                {
                    await _unitOfWork.Comments.DeleteAsync(comment.CommentId);
                }

                await _unitOfWork.Answers.DeleteAsync(answer.AnswerId);
            }

            // Delete question-tag links and questions
            foreach (var questionId in questionIds)
            {
                await _unitOfWork.QuestionTags.DeleteByQuestionIdAsync(questionId);

                // Delete question ratings
                var questionRatings = await _unitOfWork.QuestionRatings.GetByQuestionIdAsync(questionId);
                foreach (var rating in questionRatings)
                {
                    await _unitOfWork.QuestionRatings.DeleteAsync(rating.RatingId);
                }

                await _unitOfWork.Questions.DeleteAsync(questionId);
            }

            // Delete the user
            await _unitOfWork.Users.DeleteAsync(id);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
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

        if (userIdClaim == null)
            return null;

        return int.TryParse(userIdClaim.Value, out var id) ? id : null;
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