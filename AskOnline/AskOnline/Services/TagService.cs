using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;

namespace AskOnline.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public TagService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<string?> AddTagToQuestionAsync(int questionId, string tagName)
        {
            var userId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
            if (question == null)
                return "Question not found.";

            if (!isAdmin && question.UserId != userId)
                return "Forbidden";

            var tag = await _unitOfWork.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());

            if (tag == null)
            {
                tag = new Tag
                {
                    Name = tagName
                };
                await _unitOfWork.Tags.AddAsync(tag);
                await _unitOfWork.SaveChangesAsync();
            }

            // Check if tag already associated
            var questionTags = await _unitOfWork.QuestionTags.GetByQuestionIdAsync(questionId);
            if (questionTags.Any(qt => qt.TagId == tag.TagId))
                return "Tag already associated with question.";

            await _unitOfWork.QuestionTags.AddAsync(new QuestionTag
            {
                QuestionId = questionId,
                TagId = tag.TagId
            });
            await _unitOfWork.SaveChangesAsync();

            return null; // success
        }

        public async Task<TagDto?> CreateTagAsync(string name)
        {
            bool exists = await _unitOfWork.Tags
                .ExistsAsync(t => t.Name.ToLower() == name.ToLower());

            if (exists)
                return null;

            var tag = new Tag { Name = name };
            await _unitOfWork.Tags.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            return new TagDto
            {
                TagId = tag.TagId,
                Name = tag.Name
            };
        }

        public async Task<bool> DeleteTagAsync(int tagId)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(tagId);
            if (tag == null)
                return false;

            // Delete all question-tag associations first
            await _unitOfWork.QuestionTags.DeleteByQuestionIdAsync(tagId);

            // Delete the tag
            await _unitOfWork.Tags.DeleteAsync(tagId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<QuestionTag>> GetOrCreateQuestionTagsAsync(List<string> tagNames, Question question)
        {
            var normalizedNames = tagNames
                .Select(name => name.Trim().ToLower())
                .Distinct()
                .ToList();

            var existingTags = await _unitOfWork.Tags
                .FindAsync(t => normalizedNames.Contains(t.Name.ToLower()));

            var questionTags = new List<QuestionTag>();

            foreach (var tagName in normalizedNames)
            {
                var tag = existingTags
                    .FirstOrDefault(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));

                if (tag == null)
                {
                    tag = new Tag
                    {
                        Name = tagName
                    };
                    await _unitOfWork.Tags.AddAsync(tag);
                    await _unitOfWork.SaveChangesAsync();
                }

                questionTags.Add(new QuestionTag
                {
                    Tag = tag,
                    Question = question
                });
            }

            return questionTags;
        }

        public async Task<TagDto?> GetTagByIdAsync(int id)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null)
                return null;

            return new TagDto
            {
                TagId = tag.TagId,
                Name = tag.Name
            };
        }

        public async Task<List<TagDto>> GetAllTagsAsync()
        {
            var tags = await _unitOfWork.Tags.GetAllAsync();
            return tags.Select(t => new TagDto
            {
                TagId = t.TagId,
                Name = t.Name
            }).ToList();
        }

        public async Task<bool> RemoveTagFromQuestionAsync(int questionId, int tagId)
        {
            var currentUserId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
            if (question == null)
                return false;

            if (!isAdmin && question.UserId != currentUserId)
                return false;

            var questionTags = await _unitOfWork.QuestionTags.GetByQuestionIdAsync(questionId);
            var questionTag = questionTags.FirstOrDefault(qt => qt.TagId == tagId);

            if (questionTag == null)
                return false;

            await _unitOfWork.QuestionTags.DeleteAsync(questionTag);
            await _unitOfWork.SaveChangesAsync();

            await CleanupUnusedTagsAsync();

            return true;
        }

        public async Task CleanupUnusedTagsAsync()
        {
            var allTags = await _unitOfWork.Tags.GetAllAsync();
            var unusedTags = new List<Tag>();

            foreach (var tag in allTags)
            {
                var questionTags = await _unitOfWork.QuestionTags.GetByTagIdAsync(tag.TagId);
                if (!questionTags.Any())
                {
                    unusedTags.Add(tag);
                }
            }

            if (unusedTags.Any())
            {
                foreach (var tag in unusedTags)
                {
                    await _unitOfWork.Tags.DeleteAsync(tag.TagId);
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}