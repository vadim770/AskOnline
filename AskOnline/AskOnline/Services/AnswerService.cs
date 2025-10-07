using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace AskOnline.Services
{
    public class AnswerService : IAnswerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public AnswerService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<List<AnswerResponseDto>> GetAnswersForQuestion(int questionId)
        {
            var answers = await _unitOfWork.Answers.GetByQuestionIdAsync(questionId);

            return answers
                .Select(a => MapAnswerToDto(a))
                .ToList();
        }

        public async Task<AnswerResponseDto?> CreateAnswerAsync(AnswerRequestDto request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return null;

            var question = await _unitOfWork.Questions.GetByIdAsync(request.QuestionId);
            if (question == null)
                return null;

            var answer = new Answer
            {
                Body = request.Body,
                CreatedAt = DateTime.UtcNow,
                QuestionId = request.QuestionId,
                UserId = userId.Value
            };

            await _unitOfWork.Answers.AddAsync(answer);
            await _unitOfWork.SaveChangesAsync();

            var answerWithUser = await _unitOfWork.Answers.GetWithRatingsAsync(answer.AnswerId);

            return MapAnswerToDto(answerWithUser);
        }

        public async Task<IActionResult> DeleteAnswerAsync(int id)
        {
            var userId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var answer = await _unitOfWork.Answers.GetByIdAsync(id);
            if (answer == null)
                return new NotFoundResult();

            if (!isAdmin && answer.UserId != userId)
                return new ForbidResult();

            await _unitOfWork.Answers.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return new NoContentResult();
        }

        public AnswerResponseDto MapAnswerToDto(Answer answer)
        {
            var currentUserId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var upvotes = answer.Ratings?.Count(r => r.IsUpvote) ?? 0;
            var downvotes = answer.Ratings?.Count(r => !r.IsUpvote) ?? 0;

            bool? userVote = null;
            if (answer.Ratings != null && currentUserId.HasValue)
            {
                var rating = answer.Ratings.FirstOrDefault(r => r.UserId == currentUserId.Value);
                if (rating != null)
                    userVote = rating.IsUpvote;
            }

            return new AnswerResponseDto
            {
                AnswerId = answer.AnswerId,
                Body = answer.Body,
                CreatedAt = answer.CreatedAt,
                QuestionId = answer.QuestionId,
                User = answer.User != null ? _userService.MapUserDto(answer.User) : null,
                UpvoteCount = upvotes,
                DownvoteCount = downvotes,
                TotalScore = upvotes - downvotes,
                CurrentUserVote = userVote
            };
        }

        public async Task<List<AnswerResponseDto>> GetAnswersByUserIdAsync(int userId)
        {
            var answers = await _unitOfWork.Answers.GetByUserIdAsync(userId);

            return answers.Select(a => MapAnswerToDto(a)).ToList();
        }

        public async Task<AnswerUpdateDto?> UpdateAnswerAsync(int id, AnswerUpdateDto dto)
        {
            var userId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var answer = await _unitOfWork.Answers.GetByIdAsync(id);

            if (answer == null)
                return null;

            if (!isAdmin && userId != answer.UserId)
                throw new UnauthorizedAccessException("User is not authorized to update this answer.");

            answer.Body = dto.Body;

            await _unitOfWork.Answers.UpdateAsync(answer);
            await _unitOfWork.SaveChangesAsync();

            return new AnswerUpdateDto
            {
                Body = answer.Body
            };
        }
    }
}