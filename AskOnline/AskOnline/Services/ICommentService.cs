using AskOnline.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AskOnline.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentResponseDto>> GetCommentsByAnswerAsync(int answerId);
        Task<CommentResponseDto> AddCommentToAnswerAsync(int answerId, CommentCreateDto dto);
        Task<CommentResponseDto?> UpdateCommentAsync(int commentId, CommentUpdateDto dto);
        Task<bool> DeleteCommentAsync(int commentId);
    }
}
