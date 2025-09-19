namespace AskOnline.Dtos
{
    public class QuestionRatingRequestDto
    {
        public int QuestionId { get; set; }
        public bool IsUpvote { get; set; } // true for upvote, false for downvote
    }

    public class QuestionRatingResponseDto
    {
        public int RatingId { get; set; }
        public int QuestionId { get; set; }
        public bool IsUpvote { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class QuestionScoreDto
    {
        public int QuestionId { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int TotalScore { get; set; } // Upvotes - Downvotes
        public bool? UserVote { get; set; } // null = no vote, true = upvoted, false = downvoted
    }
}
