using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace AskOnline.Services
{
    public class TagService
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public TagService(AppDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<string?> AddTagToQuestionAsync(int questionId, string tagName)
        {
            var userId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var question = await _context.Questions
                .Include(q => q.QuestionTags)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null)
                return "Question not found.";

            if (!isAdmin && question.UserId != userId)
                return "Forbidden";

            // Check if tag exists (case-insensitive)
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());

            if (tag == null)
            {
                tag = new Tag
                {
                    Name = tagName
                };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            // Avoid duplicate tag
            if (question.QuestionTags.Any(qt => qt.TagId == tag.TagId))
                return "Tag already associated with question.";

            _context.QuestionTags.Add(new QuestionTag
            {
                QuestionId = question.QuestionId,
                TagId = tag.TagId
            });

            await _context.SaveChangesAsync();
            return null; // success
        }

        public async Task<TagDto?> CreateTagAsync(string name)
        {
            bool exists = await _context.Tags
                .AnyAsync(t => t.Name.ToLower() == name.ToLower());

            if (exists)
                return null;

            var tag = new Tag { Name = name };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return new TagDto
            {
                TagId = tag.TagId,
                Name = tag.Name
            };
        }



        public async Task<List<QuestionTag>> GetOrCreateQuestionTagsAsync(List<string> tagNames, Question question)
        {
            var normalizedNames = tagNames
                .Select(name => name.Trim().ToLower())
                .Distinct()
                .ToList();

            // Fetch existing tags (case-insensitive match)
            var existingTags = await _context.Tags
                .Where(t => normalizedNames.Contains(t.Name.ToLower()))
                .ToListAsync();

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
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
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
            var tag = await _context.Tags.FindAsync(id);
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
            return await _context.Tags
                .Select(t => new TagDto
                {
                    TagId = t.TagId,
                    Name = t.Name
                })
                .ToListAsync();
        }

        public async Task<bool> RemoveTagFromQuestionAsync(int questionId, int tagId)
        {
            var currentUserId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            // Fetch question and check ownership
            var question = await _context.Questions
                .Include(q => q.QuestionTags)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null)
                return false;

            if (!isAdmin && question.UserId != currentUserId)
                return false;

            var questionTag = question.QuestionTags.FirstOrDefault(qt => qt.TagId == tagId);
            if (questionTag == null)
                return false;

            _context.QuestionTags.Remove(questionTag);
            await _context.SaveChangesAsync();

            // maybe need to use cleanup unused tags

            return true;
        }



        public async Task CleanupUnusedTagsAsync()
        {
            var unusedTags = await _context.Tags
                .Where(tag => !tag.QuestionTags.Any())
                .ToListAsync();

            if (unusedTags.Any())
            {
                _context.Tags.RemoveRange(unusedTags);
                await _context.SaveChangesAsync();
            }
        }
    }
}
