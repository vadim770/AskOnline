using AskOnline.Dtos;
using AskOnline.Models;
using Microsoft.AspNetCore.Mvc;

namespace AskOnline.Services
{
    public interface IAnswerService
    {
        Task<List<AnswerResponseDto>> GetAnswersForQuestion(int questionId);
        Task<AnswerResponseDto?> CreateAnswerAsync(AnswerRequestDto request);
        Task<IActionResult> DeleteAnswerAsync(int id);
        AnswerResponseDto MapAnswerToDto(Answer answer);
        Task<List<AnswerResponseDto>> GetAnswersByUserIdAsync(int userId);
        Task<AnswerUpdateDto?> UpdateAnswerAsync(int id, AnswerUpdateDto dto);
    }
}
