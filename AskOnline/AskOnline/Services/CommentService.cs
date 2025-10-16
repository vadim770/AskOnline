using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;

namespace AskOnline.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public CommentService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByAnswerAsync(int answerId)
        {
            var comments = await _unitOfWork.Comments.GetByAnswerIdAsync(answerId);

            return comments.Select(c => new CommentResponseDto
            {
                CommentId = c.CommentId,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                User = new UserResponseDto
                {
                    UserId = c.User.UserId,
                    Username = c.User.Username
                },
                QuestionId = c.Answer.QuestionId
            });
        }

        public async Task<CommentResponseDto> AddCommentToAnswerAsync(int answerId, CommentCreateDto dto)
        {
            var currentUserId = _userService.GetCurrentUserId();
            if (currentUserId == null)
                throw new UnauthorizedAccessException("User must be logged in to add comments.");

            var answer = await _unitOfWork.Answers.GetByIdAsync(answerId);
            if (answer == null)
                throw new ArgumentException("Answer not found");

            var user = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var comment = new Comment
            {
                Text = dto.Text,
                AnswerId = answerId,
                Answer = answer,
                UserId = currentUserId.Value,
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                QuestionId = answer.QuestionId,
                User = new UserResponseDto
                {
                    UserId = user.UserId,
                    Username = user.Username
                }
            };
        }

        public async Task<CommentResponseDto?> UpdateCommentAsync(int commentId, CommentUpdateDto dto)
        {
            var currentUserId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);

            if (comment == null)
                return null;

            if (!isAdmin && comment.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            comment.Text = dto.Text;

            await _unitOfWork.Comments.UpdateAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                QuestionId = comment.Answer.QuestionId,
                User = new UserResponseDto
                {
                    UserId = comment.User.UserId,
                    Username = comment.User.Username
                }
            };
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var currentUserId = _userService.GetCurrentUserId();
            var isAdmin = _userService.IsCurrentUserAdmin();

            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (comment == null)
                return false;

            if (!isAdmin && comment.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            await _unitOfWork.Comments.DeleteAsync(commentId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByUserIdAsync(int userId)
        {
            var comments = await _unitOfWork.Comments.GetByUserIdAsync(userId);

            return comments.Select(c => new CommentResponseDto
            {
                CommentId = c.CommentId,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                User = new UserResponseDto
                {
                    UserId = c.User.UserId,
                    Username = c.User.Username
                },
                QuestionId = c.Answer.QuestionId
            });

        }
    }
}