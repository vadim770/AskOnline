﻿namespace AskOnline.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    }
}