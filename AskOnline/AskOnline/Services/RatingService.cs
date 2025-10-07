using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using AskOnline.Services;

public class RatingService : IRatingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public RatingService(IUnitOfWork unitOfWork, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
    }

    public async Task<RatingResponseDto?> CreateOrUpdateRatingAsync(RatingRequestDto request)
    {
        var userId = _userService.GetCurrentUserId();
        if (userId == null)
            return null;

        var answerExists = await _unitOfWork.Answers.ExistsAsync(a => a.AnswerId == request.AnswerId);
        if (!answerExists)
            return null;

        var existingRating = await _unitOfWork.AnswerRatings
            .GetByUserAndAnswerAsync(userId.Value, request.AnswerId);

        if (existingRating != null)
        {
            existingRating.IsUpvote = request.IsUpvote;
            existingRating.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.AnswerRatings.UpdateAsync(existingRating);
        }
        else
        {
            existingRating = new AnswerRating
            {
                AnswerId = request.AnswerId,
                UserId = userId.Value,
                IsUpvote = request.IsUpvote,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AnswerRatings.AddAsync(existingRating);
        }

        await _unitOfWork.SaveChangesAsync();

        return new RatingResponseDto
        {
            RatingId = existingRating.RatingId,
            AnswerId = existingRating.AnswerId,
            IsUpvote = existingRating.IsUpvote,
            CreatedAt = existingRating.CreatedAt
        };
    }

    public async Task<bool> DeleteRatingAsync(int answerId)
    {
        var userId = _userService.GetCurrentUserId();
        if (userId == null)
            return false;

        var rating = await _unitOfWork.AnswerRatings
            .GetByUserAndAnswerAsync(userId.Value, answerId);

        if (rating == null)
            return false;

        await _unitOfWork.AnswerRatings.DeleteAsync(rating.RatingId);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<AnswerScoreDto?> GetAnswerScoreAsync(int answerId)
    {
        var answerExists = await _unitOfWork.Answers.ExistsAsync(a => a.AnswerId == answerId);
        if (!answerExists)
            return null;

        var ratings = await _unitOfWork.AnswerRatings.GetByAnswerIdAsync(answerId);
        var ratingsList = ratings.ToList();

        var upvotes = ratingsList.Count(r => r.IsUpvote);
        var downvotes = ratingsList.Count(r => !r.IsUpvote);

        bool? userVote = null;
        var userId = _userService.GetCurrentUserId();
        if (userId != null)
        {
            var userRating = ratingsList.FirstOrDefault(r => r.UserId == userId);
            userVote = userRating?.IsUpvote;
        }

        return new AnswerScoreDto
        {
            AnswerId = answerId,
            UpvoteCount = upvotes,
            DownvoteCount = downvotes,
            TotalScore = upvotes - downvotes,
            UserVote = userVote
        };
    }
}