using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;


public class RatingService
{
    private readonly AppDbContext _context;
    private readonly UserService _userService;

    public RatingService(AppDbContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<RatingResponseDto?> CreateOrUpdateRatingAsync(RatingRequestDto request)
    {
        var userId = _userService.GetCurrentUserId();
        if (userId == null)
            return null;

        var answerExists = await _context.Answers.AnyAsync(a => a.AnswerId == request.AnswerId);
        if (!answerExists)
            return null;

        var existingRating = await _context.AnswerRatings
            .FirstOrDefaultAsync(r => r.AnswerId == request.AnswerId && r.UserId == userId.Value);

        if (existingRating != null)
        {
            existingRating.IsUpvote = request.IsUpvote;
            existingRating.CreatedAt = DateTime.UtcNow;
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
            _context.AnswerRatings.Add(existingRating);
        }

        await _context.SaveChangesAsync();

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

        var rating = await _context.AnswerRatings
            .FirstOrDefaultAsync(ar => ar.AnswerId == answerId && ar.UserId == userId.Value);

        if (rating == null)
            return false;

        _context.AnswerRatings.Remove(rating);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AnswerScoreDto?> GetAnswerScoreAsync(int answerId)
    {
        var ratings = await _context.AnswerRatings
            .Where(ar => ar.AnswerId == answerId)
            .ToListAsync();

        var upvotes = ratings.Count(r => r.IsUpvote);
        var downvotes = ratings.Count(r => !r.IsUpvote);

        bool? userVote = null;
        var userId = _userService.GetCurrentUserId();
        if (userId != null)
        {
            var userRating = ratings.FirstOrDefault(r => r.UserId == userId.Value);
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
