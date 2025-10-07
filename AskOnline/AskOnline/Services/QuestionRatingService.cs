using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;

namespace AskOnline.Services
{
    public class QuestionRatingService : IQuestionRatingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public QuestionRatingService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<QuestionRatingResponseDto?> CreateOrUpdateRatingAsync(QuestionRatingRequestDto request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return null;

            var questionExists = await _unitOfWork.Questions.ExistsAsync(q => q.QuestionId == request.QuestionId);
            if (!questionExists)
                return null;

            var existingRating = await _unitOfWork.QuestionRatings
                .GetByUserAndQuestionAsync(userId.Value, request.QuestionId);

            if (existingRating != null)
            {
                existingRating.IsUpvote = request.IsUpvote;
                existingRating.CreatedAt = DateTime.UtcNow;
                await _unitOfWork.QuestionRatings.UpdateAsync(existingRating);
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
                await _unitOfWork.QuestionRatings.AddAsync(existingRating);
            }

            await _unitOfWork.SaveChangesAsync();

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

            var rating = await _unitOfWork.QuestionRatings
                .GetByUserAndQuestionAsync(userId.Value, questionId);

            if (rating == null)
                return false;

            await _unitOfWork.QuestionRatings.DeleteAsync(rating.RatingId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<QuestionScoreDto?> GetQuestionScoreAsync(int questionId)
        {
            var questionExists = await _unitOfWork.Questions.ExistsAsync(q => q.QuestionId == questionId);
            if (!questionExists)
                return null;

            var ratings = await _unitOfWork.QuestionRatings.GetByQuestionIdAsync(questionId);
            var ratingsList = ratings.ToList();

            var upvotes = ratingsList.Count(r => r.IsUpvote);
            var downvotes = ratingsList.Count(r => !r.IsUpvote);

            bool? userVote = null;
            var userId = _userService.GetCurrentUserId();
            if (userId.HasValue)
            {
                var userRating = ratingsList.FirstOrDefault(r => r.UserId == userId.Value);
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