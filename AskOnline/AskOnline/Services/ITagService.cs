using AskOnline.Dtos;
using AskOnline.Models;

namespace AskOnline.Services
{
    public interface ITagService
    {
        Task<string?> AddTagToQuestionAsync(int questionId, string tagName);
        Task<TagDto?> CreateTagAsync(string name);
        Task<bool> DeleteTagAsync(int tagId);
        Task<List<QuestionTag>> GetOrCreateQuestionTagsAsync(List<string> tagNames, Question question);
        Task<TagDto?> GetTagByIdAsync(int id);
        Task<List<TagDto>> GetAllTagsAsync();
        Task<bool> RemoveTagFromQuestionAsync(int questionId, int tagId);
        Task CleanupUnusedTagsAsync();
    }
}
