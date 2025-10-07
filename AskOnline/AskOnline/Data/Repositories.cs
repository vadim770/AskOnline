using AskOnline.Models;
using System.Linq.Expressions;

namespace AskOnline.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteAsync(int id);
        Task DeleteAsync(string id);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // for complex queries with includes
        IQueryable<T> Query();
        IQueryable<T> Query(Expression<Func<T, bool>> predicate);
    }
}


namespace AskOnline.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
    }

    public interface IQuestionRepository : IRepository<Question>
    {
        Task<IEnumerable<Question>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Question>> GetWithTagsAsync();
        Task<Question?> GetWithTagsAndAnswersAsync(int questionId);
        Task<IEnumerable<Question>> GetByTagAsync(string tagName);
        Task<IEnumerable<Question>> GetRecentQuestionsAsync(int count = 10);
        Task<IEnumerable<Question>> SearchQuestionsAsync(string searchTerm);
        Task<IEnumerable<Question>> GetPopularQuestionsAsync(int count = 10);
    }

    public interface IAnswerRepository : IRepository<Answer>
    {
        Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId);
        Task<IEnumerable<Answer>> GetByUserIdAsync(int userId);
        Task<Answer?> GetWithRatingsAsync(int answerId);
        Task<IEnumerable<Answer>> GetTopAnswersForQuestionAsync(int questionId, int count = 5);
        Task<int> GetAnswerCountForQuestionAsync(int questionId);
    }

    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count = 20);
        Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm);
        Task<Tag> GetOrCreateTagAsync(string tagName);
    }

    public interface IQuestionTagRepository : IRepository<QuestionTag>
    {
        Task<IEnumerable<QuestionTag>> GetByQuestionIdAsync(int questionId);
        Task<IEnumerable<QuestionTag>> GetByTagIdAsync(int tagId);
        Task DeleteByQuestionIdAsync(int questionId);
        Task<bool> ExistsAsync(int questionId, int tagId);
    }

    public interface IAnswerRatingRepository : IRepository<AnswerRating>
    {
        Task<AnswerRating?> GetByUserAndAnswerAsync(int userId, int answerId);
        Task<IEnumerable<AnswerRating>> GetByAnswerIdAsync(int answerId);
        Task<IEnumerable<AnswerRating>> GetByUserIdAsync(int userId);
        Task<int> GetRatingCountForAnswerAsync(int answerId);
        Task<int> GetScoreForAnswerAsync(int answerId);
    }

    public interface IQuestionRatingRepository : IRepository<QuestionRating>
    {
        Task<QuestionRating?> GetByUserAndQuestionAsync(int userId, int questionId);
        Task<IEnumerable<QuestionRating>> GetByQuestionIdAsync(int questionId);
        Task<IEnumerable<QuestionRating>> GetByUserIdAsync(int userId);
        Task<int> GetRatingCountForQuestionAsync(int questionId);
        Task<int> GetScoreForQuestionAsync(int questionId);
    }

    public interface ICommentRepository : IRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetByAnswerIdAsync(int answerId);
        Task<IEnumerable<Comment>> GetByUserIdAsync(int userId);
        Task<int> GetCommentCountForAnswerAsync(int answerId);
    }
}