namespace AskOnline.Models
{
    public class QuestionRating
    {
        public int RatingId { get; set; }
        public int QuestionId { get; set; }
        public int UserId { get; set; }
        public bool IsUpvote { get; set; } // true = upvote, false = downvote
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Question Question { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
