using AskOnline.Dtos;

public class AnswerResponseDto
{
    public int AnswerId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int QuestionId { get; set; }
    public UserPublicDto User { get; set; } = new();
    public int UpvoteCount { get; set; }
    public int DownvoteCount { get; set; }
    public int TotalScore { get; set; }
    public bool? CurrentUserVote { get; set; } // null if user hasn't voted or is not logged in
}
