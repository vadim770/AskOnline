using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace AskOnline.Services
{
    public class QuestionService
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;
        private readonly TagService _tagService;
        private readonly AnswerService _answerService;

        public QuestionService(AppDbContext context, UserService userService, TagService tagService, AnswerService answerService)
        {
            _context = context;
            _userService = userService;
            _tagService = tagService;
            _answerService = answerService;
        }

        public async Task<QuestionResponseDto?> CreateQuestionAsync(QuestionRequestDto request)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                return null;

            // Make sure user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId.Value);
            if (!userExists)
                return null;

            var question = new Question
            {
                Title = request.Title,
                Body = request.Body,
                CreatedAt = DateTime.UtcNow,
                UserId = userId.Value,
                QuestionTags = new List<QuestionTag>()
            };

            question.QuestionTags = await _tagService.GetOrCreateQuestionTagsAsync(request.TagNames, question);

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var fullQuestion = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.QuestionId == question.QuestionId);

            return MapQuestionToDto(fullQuestion);
        }

        public async Task<List<QuestionResponseDto>> GetAllQuestionsAsync()
        {
            var isAdmin = _userService.IsCurrentUserAdmin();
            var currentUserId = _userService.GetCurrentUserId();

            var questions = await _context.Questions
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Ratings)
                .ToListAsync();

            var result = new List<QuestionResponseDto>();

            foreach (var q in questions)
            {
                var answerDtos = q.Answers
                    .Select(a => _answerService.MapAnswerToDto(a))
                    .ToList();

                result.Add(MapQuestionToDto(q, answerDtos));
            }

            return result;
        }

        public async Task<QuestionResponseDto?> GetQuestionByIdAsync(int questionId)
        {
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Ratings)
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null)
                return null;

            var isAdmin = _userService.IsCurrentUserAdmin();
            var currentUserId = _userService.GetCurrentUserId();

            var answerDtos = question.Answers
                .Select(a => _answerService.MapAnswerToDto(a))
                .ToList();

            return MapQuestionToDto(question, answerDtos);
        }

        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            var userId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var question = await _context.Questions.FindAsync(questionId);
            if (question == null)
                return false;

            if (!isAdmin && question.UserId != userId)
                throw new UnauthorizedAccessException("User is not authorized to delete this question.");

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            await _tagService.CleanupUnusedTagsAsync();

            return true;
        }

        public QuestionResponseDto MapQuestionToDto(
            Question question,
            List<AnswerResponseDto>? answerDtos = null
        )
        {
            return new QuestionResponseDto
            {
                QuestionId = question.QuestionId,
                Title = question.Title,
                Body = question.Body,
                CreatedAt = question.CreatedAt,
                User = _userService.MapUserDto(question.User),
                Answers = answerDtos ?? new List<AnswerResponseDto>(),
                Tags = question.QuestionTags.Select(qt => new TagDto
                {
                    TagId = qt.Tag.TagId,
                    Name = qt.Tag.Name
                }).ToList()
            };
        }




    }
}
