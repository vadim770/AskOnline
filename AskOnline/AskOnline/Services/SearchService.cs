using AskOnline.Data;
using AskOnline.Dtos;
using AskOnline.Models;
using System.Text.RegularExpressions;

namespace AskOnline.Services
{
    public class SearchService : ISearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public SearchService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<SearchResultDto> SearchQuestionsAsync(SearchRequestDto request)
        {
            // Extract tags from query using [tagname] syntax
            var (searchText, extractedTags) = ExtractTagsFromQuery(request.Query);

            var (questions, totalCount) = await _unitOfWork.Questions.SearchAsync(
                searchText,
                extractedTags,
                request.Filters?.Tags,
                request.Filters,
                request.SortBy,
                request.Page,
                request.PageSize
            );

            var questionDtos = questions.Select(MapToSearchDto).ToList();

            return new SearchResultDto
            {
                Questions = questionDtos,
                TotalCount = totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }

        private (string searchText, List<string> tags) ExtractTagsFromQuery(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return (string.Empty, new List<string>());

            var tagPattern = @"\[([^\[\]]+)\]";
            var matches = Regex.Matches(query, tagPattern);

            var extractedTags = matches
                .Cast<Match>()
                .Select(m => m.Groups[1].Value.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var searchText = Regex.Replace(query, tagPattern, "").Trim();
            searchText = Regex.Replace(searchText, @"\s+", " ");

            return (searchText, extractedTags);
        }

        private QuestionSearchDto MapToSearchDto(Question question)
        {
            var answerCount = question.Answers?.Count ?? 0;
            var score = (question.Ratings?.Count(r => r.IsUpvote) ?? 0) -
                       (question.Ratings?.Count(r => !r.IsUpvote) ?? 0);

            var lastActivity = question.Answers?.Any() == true
                ? question.Answers.Max(a => a.CreatedAt)
                : question.CreatedAt;

            var hasUpvotedAnswers = question.Answers?.Any(a =>
                a.Ratings?.Any(r => r.IsUpvote) == true
            ) ?? false;

            return new QuestionSearchDto
            {
                QuestionId = question.QuestionId,
                Title = question.Title,
                Body = TruncateText(question.Body, 200),
                CreatedAt = question.CreatedAt,
                User = _userService.MapUserDto(question.User),
                Tags = question.QuestionTags?.Select(qt => new TagDto
                {
                    TagId = qt.Tag.TagId,
                    Name = qt.Tag.Name
                }).ToList() ?? new List<TagDto>(),
                AnswerCount = answerCount,
                Score = score,
                LastActivity = lastActivity,
                HasUpvotedAnswers = hasUpvotedAnswers
            };
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength).Trim() + "...";
        }
    }
}