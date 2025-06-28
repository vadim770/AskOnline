using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AskOnline.Services;

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

        public async Task<List<AnswerResponseDto>> GetAnswersForQuestion(int questionId, bool isAdmin, int? currentUserId)
        {
            var answers = await _context.Answers
                .Where(a => a.QuestionId == questionId)
                .Include(a => a.User)
                .Include(a => a.Ratings)
                .ToListAsync();

            return answers
                .Select(a => MapAnswerToDto(a, isAdmin, currentUserId))
                .ToList();
        }


        public AnswerResponseDto MapAnswerToDto(Answer answer, bool isAdmin, int? currentUserId)
        {
            var upvotes = answer.Ratings?.Count(r => r.IsUpvote) ?? 0;
            var downvotes = answer.Ratings?.Count(r => !r.IsUpvote) ?? 0;

            bool? userVote = null;
            if (currentUserId.HasValue)
            {
                userVote = answer.Ratings?
                    .FirstOrDefault(r => r.UserId == currentUserId.Value)?.IsUpvote;
            }

            return new AnswerResponseDto
            {
                AnswerId = answer.AnswerId,
                Body = answer.Body,
                CreatedAt = answer.CreatedAt,
                QuestionId = answer.QuestionId,
                User = _userService.MapUserDto(answer.User, isAdmin),
                UpvoteCount = upvotes,
                DownvoteCount = downvotes,
                TotalScore = upvotes - downvotes,
                CurrentUserVote = userVote
            };
        }



    }
}
