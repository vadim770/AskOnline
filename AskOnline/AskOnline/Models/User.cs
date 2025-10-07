using System.Text.Json.Serialization;

namespace AskOnline.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";


        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Question>? Questions { get; set; }
        public ICollection<Answer>? Answers { get; set; }

        public ICollection<AnswerRating> AnswerRatings { get; set; } = new List<AnswerRating>();
        public ICollection<QuestionRating> QuestionRatings { get; set; } = new List<QuestionRating>();
    }
}
