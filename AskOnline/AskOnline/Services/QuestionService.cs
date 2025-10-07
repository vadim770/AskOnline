using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;

namespace AskOnline.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ITagService _tagService;
        private readonly IAnswerService _answerService;

        public QuestionService(IUnitOfWork unitOfWork, IUserService userService, ITagService tagService, IAnswerService answerService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _tagService = tagService;
            _answerService = answerService;
        }

        public async Task<QuestionResponseDto?> CreateQuestionAsync(QuestionRequestDto request)
        {
            var userId = _userService.GetCurrentUserId();

            var userExists = await _unitOfWork.Users.ExistsAsync(u => u.UserId == userId);
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

            await _unitOfWork.Questions.AddAsync(question);
            await _unitOfWork.SaveChangesAsync();

            var fullQuestion = await _unitOfWork.Questions.GetWithTagsAndAnswersAsync(question.QuestionId);
            return MapQuestionToDto(fullQuestion);
        }

        public async Task<List<QuestionResponseDto>> GetAllQuestionsAsync()
        {
            var isAdmin = _userService.IsCurrentUserAdmin();
            var currentUserId = _userService.GetCurrentUserId();

            var questions = await _unitOfWork.Questions.GetWithTagsAsync();

            var result = new List<QuestionResponseDto>();

            foreach (var q in questions)
            {
                var answerDtos = q.Answers?
                    .Select(a => _answerService.MapAnswerToDto(a))
                    .ToList() ?? new List<AnswerResponseDto>();

                result.Add(MapQuestionToDto(q, answerDtos));
            }

            return result;
        }

        public async Task<QuestionResponseDto?> GetQuestionByIdAsync(int questionId)
        {
            var question = await _unitOfWork.Questions.GetWithTagsAndAnswersAsync(questionId);

            if (question == null)
                return null;

            var isAdmin = _userService.IsCurrentUserAdmin();
            var currentUserId = _userService.GetCurrentUserId();

            var answerDtos = question.Answers?
                .Select(a => _answerService.MapAnswerToDto(a))
                .ToList() ?? new List<AnswerResponseDto>();

            return MapQuestionToDto(question, answerDtos);
        }

        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            var userId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
            if (question == null)
                return false;

            if (!isAdmin && question.UserId != userId)
                throw new UnauthorizedAccessException("User is not authorized to delete this question.");

            await _unitOfWork.Questions.DeleteAsync(questionId);
            await _unitOfWork.SaveChangesAsync();

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
                Tags = question.QuestionTags?.Select(qt => new TagDto
                {
                    TagId = qt.Tag.TagId,
                    Name = qt.Tag.Name
                }).ToList() ?? new List<TagDto>()
            };
        }

        public async Task<List<QuestionResponseDto>> GetQuestionsByUserIdAsync(int userId)
        {
            try
            {
                var questions = await _unitOfWork.Questions.GetByUserIdAsync(userId);

                if (questions == null || !questions.Any())
                    return new List<QuestionResponseDto>();

                var result = new List<QuestionResponseDto>();
                foreach (var q in questions)
                {
                    if (q == null) continue;

                    var answerDtos = q.Answers?
                        .Where(a => a != null)
                        .Select(a => _answerService.MapAnswerToDto(a))
                        .ToList() ?? new List<AnswerResponseDto>();

                    result.Add(MapQuestionToDto(q, answerDtos));
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving questions for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task<QuestionUpdateDto?> UpdateQuestionAsync(int id, QuestionUpdateDto dto)
        {
            var userId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var question = await _unitOfWork.Questions.GetWithTagsAndAnswersAsync(id);

            if (question == null)
                return null;

            if (!isAdmin && question.UserId != userId)
                throw new UnauthorizedAccessException("User is not authorized to update this question.");

            question.Title = dto.Title;
            question.Body = dto.Body;

            var incomingTagNames = dto.Tags
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var currentTagNames = question.QuestionTags
                .Select(qt => qt.Tag.Name)
                .ToList();

            var tagsToRemove = question.QuestionTags
                .Where(qt => !incomingTagNames.Contains(qt.Tag.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var tagNamesToAdd = incomingTagNames
                .Where(tagName => !currentTagNames.Contains(tagName, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Remove old tags
            foreach (var questionTag in tagsToRemove)
            {
                question.QuestionTags.Remove(questionTag);
                await _unitOfWork.QuestionTags.DeleteAsync(questionTag);
            }

            // Add new tags
            if (tagNamesToAdd.Any())
            {
                var newQuestionTags = await _tagService.GetOrCreateQuestionTagsAsync(tagNamesToAdd, question);
                foreach (var newQuestionTag in newQuestionTags)
                {
                    question.QuestionTags.Add(newQuestionTag);
                }
            }

            await _unitOfWork.Questions.UpdateAsync(question);
            await _unitOfWork.SaveChangesAsync();

            await _tagService.CleanupUnusedTagsAsync();

            return new QuestionUpdateDto
            {
                Title = question.Title,
                Body = question.Body,
                Tags = question.QuestionTags.Select(qt => qt.Tag.Name).ToList()
            };
        }

        public async Task<List<QuestionResponseDto>> GetRecentQuestionsAsync(int limit = 20)
        {
            var questions = await _unitOfWork.Questions.GetRecentQuestionsAsync(limit);

            var result = new List<QuestionResponseDto>();

            foreach (var q in questions)
            {
                var answerDtos = q.Answers?
                    .Select(a => _answerService.MapAnswerToDto(a))
                    .ToList() ?? new List<AnswerResponseDto>();

                result.Add(MapQuestionToDto(q, answerDtos));
            }

            return result;
        }
    }
}