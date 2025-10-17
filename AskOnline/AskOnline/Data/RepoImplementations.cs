using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AskOnline.Data.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public virtual async Task DeleteAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet;
        }

        public virtual IQueryable<T> Query(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.Username == username);
        }

    }

    public class QuestionRepository : Repository<Question>, IQuestionRepository
    {
        public QuestionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Question>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(q => q.User)
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Question>> GetWithTagsAsync()
        {
            return await _dbSet
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.User)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Question?> GetWithTagsAndAnswersAsync(int questionId)
        {
            return await _dbSet
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Ratings)
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);
        }

        public async Task<IEnumerable<Question>> GetByTagAsync(string tagName)
        {
            return await _dbSet
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.User)
                .Where(q => q.QuestionTags.Any(qt => qt.Tag.Name == tagName))
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Question>> GetRecentQuestionsAsync(int count = 10)
        {
            return await _dbSet
                .Include(q => q.User)
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Question>> SearchQuestionsAsync(string searchTerm)
        {
            return await _dbSet
                .Include(q => q.User)
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Where(q => q.Title.Contains(searchTerm) || q.Body.Contains(searchTerm))
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Question>> GetPopularQuestionsAsync(int count = 10)
        {
            return await _dbSet
                .Include(q => q.User)
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.Answers.Count)
                .ThenByDescending(q => q.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Question> questions, int totalCount)> SearchAsync(
            string? searchText,
            List<string> extractedTags,
            List<string>? filterTags,
            SearchFilters? filters,
            SearchSortBy sortBy,
            int page,
            int pageSize)
        {
            IQueryable<Question> query = _dbSet
                .Include(q => q.User)
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Ratings)
                .Include(q => q.Ratings);

            // Apply text search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchTerms = searchText.ToLower()
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(term => $"%{term}%")
                    .ToList();

                query = query.Where(q =>
                    searchTerms.Any(pattern =>
                        EF.Functions.Like(q.Title.ToLower(), pattern) ||
                        EF.Functions.Like(q.Body.ToLower(), pattern)
                    )
                );
            }

            // Apply tag filtering from extracted tags
            if (extractedTags.Any())
            {
                foreach (var tag in extractedTags)
                {
                    var tagLower = tag.ToLower();
                    query = query.Where(q =>
                        q.QuestionTags.Any(qt =>
                            EF.Functions.Like(qt.Tag.Name.ToLower(), tagLower)
                        )
                    );
                }
            }

            // Apply additional tag filtering
            if (filterTags != null && filterTags.Any())
            {
                foreach (var tag in filterTags)
                {
                    var tagLower = tag.ToLower();
                    query = query.Where(q =>
                        q.QuestionTags.Any(qt =>
                            EF.Functions.Like(qt.Tag.Name.ToLower(), tagLower)
                        )
                    );
                }
            }

            // Apply other filters
            if (filters != null)
            {
                if (filters.NoAnswers == true)
                {
                    query = query.Where(q => !q.Answers.Any());
                }

                if (filters.NoUpvotedAnswers == true)
                {
                    query = query.Where(q =>
                        !q.Answers.Any(a => a.Ratings.Any(r => r.IsUpvote))
                    );
                }

                if (filters.OlderThanDays.HasValue && filters.OlderThanDays.Value > 0)
                {
                    var cutoffDate = DateTime.UtcNow.AddDays(-filters.OlderThanDays.Value);
                    query = query.Where(q => q.CreatedAt < cutoffDate);
                }
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy switch
            {
                SearchSortBy.Newest => query.OrderByDescending(q => q.CreatedAt),
                SearchSortBy.Score => query.OrderByDescending(q =>
                    q.Ratings.Count(r => r.IsUpvote) - q.Ratings.Count(r => !r.IsUpvote)
                ),
                SearchSortBy.Active => query.OrderByDescending(q =>
                    q.Answers.Any() ? q.Answers.Max(a => a.CreatedAt) : q.CreatedAt
                ),
                _ => query.OrderByDescending(q => q.CreatedAt)
            };

            // Apply pagination
            var questions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (questions, totalCount);
        }
    }

    public class AnswerRepository : Repository<Answer>, IAnswerRepository
    {
        public AnswerRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Include(a => a.Ratings)
                .Include(a => a.Comments)
                .Where(a => a.QuestionId == questionId)
                .OrderByDescending(a => a.Ratings.Count(r => r.IsUpvote) - a.Ratings.Count(r => !r.IsUpvote))
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Answer>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(a => a.Question)
                .Include(a => a.Ratings)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Answer?> GetWithRatingsAsync(int answerId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Include(a => a.Ratings)
                    .ThenInclude(r => r.User)
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.AnswerId == answerId);
        }

        public async Task<IEnumerable<Answer>> GetTopAnswersForQuestionAsync(int questionId, int count = 5)
        {
            return await _dbSet
                .Include(a => a.User)
                .Include(a => a.Ratings)
                .Where(a => a.QuestionId == questionId)
                .OrderByDescending(a => a.Ratings.Count(r => r.IsUpvote) - a.Ratings.Count(r => !r.IsUpvote))
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetAnswerCountForQuestionAsync(int questionId)
        {
            return await _dbSet.CountAsync(a => a.QuestionId == questionId);
        }
    }

    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(AppDbContext context) : base(context) { }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count = 20)
        {
            return await _dbSet
                .Include(t => t.QuestionTags)
                .OrderByDescending(t => t.QuestionTags.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm)
        {
            return await _dbSet
                .Where(t => t.Name.Contains(searchTerm))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag> GetOrCreateTagAsync(string tagName)
        {
            var existingTag = await GetByNameAsync(tagName);
            if (existingTag != null)
            {
                return existingTag;
            }

            var newTag = new Tag { Name = tagName };
            await AddAsync(newTag);
            return newTag;
        }

        public async Task<Tag?> GetTagWithQuestionTagsAsync(int tagId)
        {
            return await _dbSet
                .Include(t => t.QuestionTags)
                .FirstOrDefaultAsync(t => t.TagId == tagId);
        }
    }

    public class QuestionTagRepository : Repository<QuestionTag>, IQuestionTagRepository
    {
        public QuestionTagRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<QuestionTag>> GetByQuestionIdAsync(int questionId)
        {
            return await _dbSet
                .Include(qt => qt.Tag)
                .Where(qt => qt.QuestionId == questionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuestionTag>> GetByTagIdAsync(int tagId)
        {
            return await _dbSet
                .Include(qt => qt.Question)
                .Where(qt => qt.TagId == tagId)
                .ToListAsync();
        }

        public async Task DeleteByQuestionIdAsync(int questionId)
        {
            var questionTags = await _dbSet.Where(qt => qt.QuestionId == questionId).ToListAsync();
            _dbSet.RemoveRange(questionTags);
        }

        public async Task<bool> ExistsAsync(int questionId, int tagId)
        {
            return await _dbSet.AnyAsync(qt => qt.QuestionId == questionId && qt.TagId == tagId);
        }
    }

    public class AnswerRatingRepository : Repository<AnswerRating>, IAnswerRatingRepository
    {
        public AnswerRatingRepository(AppDbContext context) : base(context) { }

        public async Task<AnswerRating?> GetByUserAndAnswerAsync(int userId, int answerId)
        {
            return await _dbSet.FirstOrDefaultAsync(ar => ar.UserId == userId && ar.AnswerId == answerId);
        }

        public async Task<IEnumerable<AnswerRating>> GetByAnswerIdAsync(int answerId)
        {
            return await _dbSet
                .Include(ar => ar.User)
                .Where(ar => ar.AnswerId == answerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnswerRating>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(ar => ar.Answer)
                .Where(ar => ar.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> GetRatingCountForAnswerAsync(int answerId)
        {
            return await _dbSet.CountAsync(ar => ar.AnswerId == answerId);
        }

        public async Task<int> GetScoreForAnswerAsync(int answerId)
        {
            var ratings = await _dbSet.Where(ar => ar.AnswerId == answerId).ToListAsync();
            if (!ratings.Any()) return 0;

            return ratings.Count(r => r.IsUpvote) - ratings.Count(r => !r.IsUpvote);
        }
    }

    public class QuestionRatingRepository : Repository<QuestionRating>, IQuestionRatingRepository
    {
        public QuestionRatingRepository(AppDbContext context) : base(context) { }

        public async Task<QuestionRating?> GetByUserAndQuestionAsync(int userId, int questionId)
        {
            return await _dbSet.FirstOrDefaultAsync(qr => qr.UserId == userId && qr.QuestionId == questionId);
        }

        public async Task<IEnumerable<QuestionRating>> GetByQuestionIdAsync(int questionId)
        {
            return await _dbSet
                .Include(qr => qr.User)
                .Where(qr => qr.QuestionId == questionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuestionRating>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(qr => qr.Question)
                .Where(qr => qr.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> GetRatingCountForQuestionAsync(int questionId)
        {
            return await _dbSet.CountAsync(qr => qr.QuestionId == questionId);
        }

        public async Task<int> GetScoreForQuestionAsync(int questionId)
        {
            var ratings = await _dbSet.Where(qr => qr.QuestionId == questionId).ToListAsync();
            if (!ratings.Any()) return 0;

            return ratings.Count(r => r.IsUpvote) - ratings.Count(r => !r.IsUpvote);
        }
    }

    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext context) : base(context) { }

        public override async Task<Comment?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Answer)
                .FirstOrDefaultAsync(c => c.CommentId == id);
        }

        public async Task<IEnumerable<Comment>> GetByAnswerIdAsync(int answerId)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Answer)
                .Where(c => c.AnswerId == answerId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(c => c.Answer)
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetCommentCountForAnswerAsync(int answerId)
        {
            return await _dbSet.CountAsync(c => c.AnswerId == answerId);
        }
    }
}