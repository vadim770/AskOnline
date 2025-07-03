using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AskOnline.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AskOnline.Services
{
    public class AnswerService
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public AnswerService(AppDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<List<AnswerResponseDto>> GetAnswersForQuestion(int questionId)
        {
            var answers = await _context.Answers
                .Where(a => a.QuestionId == questionId)
                .Include(a => a.User)
                .Include(a => a.Ratings)
                .ToListAsync();

            return answers
                .Select(a => MapAnswerToDto(a))
                .ToList();
        }

        public async Task<AnswerResponseDto?> CreateAnswerAsync(AnswerRequestDto request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return null;
            var isAdmin = _userService.IsCurrentUserAdmin();

            var question = await _context.Questions.FindAsync(request.QuestionId);
            if (question == null)
                return null;

            var answer = new Answer
            {
                Body = request.Body,
                CreatedAt = DateTime.UtcNow,
                QuestionId = request.QuestionId,
                UserId = userId.Value
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            var answerWithUser = await _context.Answers
                .Include(a => a.User)
                .Include(a => a.Ratings)
                .FirstOrDefaultAsync(a => a.AnswerId == answer.AnswerId);

            return MapAnswerToDto(answerWithUser);
        }

        public async Task<IActionResult> DeleteAnswerAsync(int id)
        {
            var userId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var answer = await _context.Answers.FindAsync(id);
            if (answer == null)
                return new NotFoundResult();

            if (!isAdmin && answer.UserId != userId)
                return new ForbidResult();

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        public AnswerResponseDto MapAnswerToDto(Answer answer)
        {
            var currentUserId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var upvotes = answer.Ratings?.Count(r => r.IsUpvote) ?? 0;
            var downvotes = answer.Ratings?.Count(r => !r.IsUpvote) ?? 0;

            bool? userVote = null;
            if (currentUserId.HasValue)
            {
                userVote = answer.Ratings?
                    .FirstOrDefault(r => r.UserId == currentUserId.Value)
                    ?.IsUpvote;
            }

            return new AnswerResponseDto
            {
                AnswerId = answer.AnswerId,
                Body = answer.Body,
                CreatedAt = answer.CreatedAt,
                QuestionId = answer.QuestionId,
                User = _userService.MapUserDto(answer.User),
                UpvoteCount = upvotes,
                DownvoteCount = downvotes,
                TotalScore = upvotes - downvotes,
                CurrentUserVote = userVote
            };
        }

    }
}
