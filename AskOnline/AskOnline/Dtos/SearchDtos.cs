using AskOnline.Dtos;

namespace AskOnline.Dtos
{
    public class SearchRequestDto
    {
        public string? Query { get; set; }
        public SearchSortBy SortBy { get; set; } = SearchSortBy.Newest;
        public SearchFilters? Filters { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
    }

    public class SearchFilters
    {
        public bool? NoAnswers { get; set; }
        public bool? NoUpvotedAnswers { get; set; }
        public int? OlderThanDays { get; set; }
        public List<string>? Tags { get; set; }
    }

    public enum SearchSortBy
    {
        Newest,
        Active,
        Score
    }

    public class SearchResultDto
    {
        public List<QuestionSearchDto> Questions { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
    }

    public class QuestionSearchDto
    {
        public int QuestionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserResponseDto User { get; set; } = null!;
        public List<TagDto> Tags { get; set; } = new();
        public int AnswerCount { get; set; }
        public int Score { get; set; }
        public DateTime? LastActivity { get; set; }
        public bool HasUpvotedAnswers { get; set; }
    }
}