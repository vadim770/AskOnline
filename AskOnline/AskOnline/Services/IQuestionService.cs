using AskOnline.Dtos;
using AskOnline.Models;

namespace AskOnline.Services
{
    public interface IQuestionService
    {
        Task<QuestionResponseDto?> CreateQuestionAsync(QuestionRequestDto request);
        Task<List<QuestionResponseDto>> GetAllQuestionsAsync();
        Task<QuestionResponseDto?> GetQuestionByIdAsync(int questionId);
        Task<bool> DeleteQuestionAsync(int questionId);
        Task<QuestionUpdateDto?> UpdateQuestionAsync(int id, QuestionUpdateDto dto);
        Task<List<QuestionResponseDto>> GetRecentQuestionsAsync(int limit = 20);
        Task<List<QuestionResponseDto>> GetQuestionsByUserIdAsync(int userId);
        QuestionResponseDto MapQuestionToDto(Question question, List<AnswerResponseDto>? answerDtos = null);
    }
}
