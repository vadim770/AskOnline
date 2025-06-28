namespace AskOnline.Dtos
{
    public class RatingRequestDto
    {
        public int AnswerId { get; set; }
        public bool IsUpvote { get; set; } // true for upvote, false for downvote
    }

    public class RatingResponseDto
    {
        public int RatingId { get; set; }
        public int AnswerId { get; set; }
        public bool IsUpvote { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AnswerScoreDto
    {
        public int AnswerId { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int TotalScore { get; set; } // Upvotes - Downvotes
        public bool? UserVote { get; set; } // null = no vote, true = upvoted, false = downvoted
    }
}