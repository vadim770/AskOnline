using AskOnline.Dtos;
using System.Threading.Tasks;

public interface IRatingService
{
    Task<RatingResponseDto?> CreateOrUpdateRatingAsync(RatingRequestDto request);
    Task<bool> DeleteRatingAsync(int answerId);
    Task<AnswerScoreDto?> GetAnswerScoreAsync(int answerId);
}
