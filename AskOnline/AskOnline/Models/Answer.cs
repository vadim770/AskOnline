using System.ComponentModel.DataAnnotations.Schema;

namespace AskOnline.Models
{
    public class Answer
    {
        public int AnswerId { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("QuestionId")]
        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        // Navigation property for ratings
        public ICollection<AnswerRating> Ratings { get; set; } = new List<AnswerRating>();

        // Computed property for score (optional, for convenience)
        public int Score => Ratings?.Count(r => r.IsUpvote) - Ratings?.Count(r => !r.IsUpvote) ?? 0;
    }
}
