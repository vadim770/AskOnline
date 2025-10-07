using AskOnline.Dtos;
using System.Threading.Tasks;

namespace AskOnline.Services
{
    public interface IQuestionRatingService
    {
        Task<QuestionRatingResponseDto?> CreateOrUpdateRatingAsync(QuestionRatingRequestDto request);
        Task<bool> DeleteRatingAsync(int questionId);
        Task<QuestionScoreDto?> GetQuestionScoreAsync(int questionId);
    }
}
