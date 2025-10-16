using AskOnline.Dtos;

namespace AskOnline.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentResponseDto>> GetCommentsByAnswerAsync(int answerId);
        Task<CommentResponseDto> AddCommentToAnswerAsync(int answerId, CommentCreateDto dto);
        Task<CommentResponseDto?> UpdateCommentAsync(int commentId, CommentUpdateDto dto);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<IEnumerable<CommentResponseDto>> GetCommentsByUserIdAsync(int userId);
    }
}
