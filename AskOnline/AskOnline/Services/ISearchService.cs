using AskOnline.Dtos;
using System.Threading.Tasks;

namespace AskOnline.Services
{
    public interface ISearchService
    {
        /// <summary>
        /// Searches for questions based on the given request parameters.
        /// </summary>
        /// <param name="request">Search request containing query, filters, pagination, and sorting options.</param>
        /// <returns>A <see cref="SearchResultDto"/> containing the matching questions, total count, and pagination details.</returns>
        Task<SearchResultDto> SearchQuestionsAsync(SearchRequestDto request);
    }
}
