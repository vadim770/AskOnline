namespace AskOnline.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int AnswerId { get; set; }
        public Answer Answer { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
