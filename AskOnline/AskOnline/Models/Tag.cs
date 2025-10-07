namespace AskOnline.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    }
}