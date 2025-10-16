using AskOnline.Data.Repositories;
using AskOnline.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore.Storage;

namespace AskOnline.Data
{

    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IQuestionRepository Questions { get; }
        IAnswerRepository Answers { get; }
        ITagRepository Tags { get; }
        IQuestionTagRepository QuestionTags { get; }
        IAnswerRatingRepository AnswerRatings { get; }
        IQuestionRatingRepository QuestionRatings { get; }
        ICommentRepository Comments { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }


    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // lazy initialization
        private IUserRepository? _users;
        private IQuestionRepository? _questions;
        private IAnswerRepository? _answers;
        private ITagRepository? _tags;
        private IQuestionTagRepository? _questionTags;
        private IAnswerRatingRepository? _answerRatings;
        private IQuestionRatingRepository? _questionRatings;
        private ICommentRepository? _comments;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IQuestionRepository Questions => _questions ??= new QuestionRepository(_context);
        public IAnswerRepository Answers => _answers ??= new AnswerRepository(_context);
        public ITagRepository Tags => _tags ??= new TagRepository(_context);
        public IQuestionTagRepository QuestionTags => _questionTags ??= new QuestionTagRepository(_context);
        public IAnswerRatingRepository AnswerRatings => _answerRatings ??= new AnswerRatingRepository(_context);
        public IQuestionRatingRepository QuestionRatings => _questionRatings ??= new QuestionRatingRepository(_context);
        public ICommentRepository Comments => _comments ??= new CommentRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}