namespace AskOnline.Models
{
    public class AnswerRating
    {
        public int RatingId { get; set; }
        public int AnswerId { get; set; }
        public int UserId { get; set; }
        public bool IsUpvote { get; set; } // true = upvote, false = downvote
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Answer Answer { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}