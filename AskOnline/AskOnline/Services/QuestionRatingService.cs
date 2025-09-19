using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace AskOnline.Services
{
    public class QuestionRatingService
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public QuestionRatingService(AppDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<QuestionRatingResponseDto?> CreateOrUpdateRatingAsync(QuestionRatingRequestDto request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return null;

            var questionExists = await _context.Questions.AnyAsync(q => q.QuestionId == request.QuestionId);
            if (!questionExists)
                return null;

            var existingRating = await _context.QuestionRatings
                .FirstOrDefaultAsync(r => r.QuestionId == request.QuestionId && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.IsUpvote = request.IsUpvote;
                existingRating.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                existingRating = new QuestionRating
                {
                    QuestionId = request.QuestionId,
                    UserId = userId.Value,
                    IsUpvote = request.IsUpvote,
                    CreatedAt = DateTime.UtcNow
                };
                _context.QuestionRatings.Add(existingRating);
            }

            await _context.SaveChangesAsync();

            return new QuestionRatingResponseDto
            {
                RatingId = existingRating.RatingId,
                QuestionId = existingRating.QuestionId,
                IsUpvote = existingRating.IsUpvote,
                CreatedAt = existingRating.CreatedAt
            };
        }

        public async Task<bool> DeleteRatingAsync(int questionId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return false;

            var rating = await _context.QuestionRatings
                .FirstOrDefaultAsync(qr => qr.QuestionId == questionId && qr.UserId == userId);

            if (rating == null)
                return false;

            _context.QuestionRatings.Remove(rating);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<QuestionScoreDto?> GetQuestionScoreAsync(int questionId)
        {
            var questionExists = await _context.Questions.AnyAsync(q => q.QuestionId == questionId);
            if (!questionExists)
                return null;

            var ratings = await _context.QuestionRatings
                .Where(qr => qr.QuestionId == questionId)
                .ToListAsync();

            var upvotes = ratings.Count(r => r.IsUpvote);
            var downvotes = ratings.Count(r => !r.IsUpvote);

            bool? userVote = null;
            var userId = _userService.GetCurrentUserId();

            if (userId.HasValue)
            {
                var userRating = ratings.FirstOrDefault(r => r.UserId == userId.Value);
                userVote = userRating?.IsUpvote;
            }

            return new QuestionScoreDto
            {
                QuestionId = questionId,
                UpvoteCount = upvotes,
                DownvoteCount = downvotes,
                TotalScore = upvotes - downvotes,
                UserVote = userVote
            };
        }
    }
}
