using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace AskOnline.Services
{
    public class CommentService
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public CommentService(AppDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByAnswerAsync(int answerId)
        {
            return await _context.Comments
                .Where(c => c.AnswerId == answerId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponseDto
                {
                    CommentId = c.CommentId,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    User = new UserResponseDto
                    {
                        UserId = c.User.UserId,
                        Username = c.User.Username
                    }
                })
                .ToListAsync();
        }

        public async Task<CommentResponseDto> AddCommentToAnswerAsync(int answerId, CommentCreateDto dto)
        {
            var currentUserId = _userService.GetCurrentUserId();
            if (currentUserId == null)
                throw new UnauthorizedAccessException("User must be logged in to add comments.");

            var answer = await _context.Answers.FindAsync(answerId);
            if (answer == null)
                throw new ArgumentException("Answer not found");

            var user = await _context.Users.FindAsync(currentUserId.Value);
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

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
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

            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
                return null;

            if (!isAdmin && comment.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            comment.Text = dto.Text;
            await _context.SaveChangesAsync();

            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
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

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
            if (comment == null)
                return false;

            if (!isAdmin && comment.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
