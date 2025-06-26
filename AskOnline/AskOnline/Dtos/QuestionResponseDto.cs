namespace AskOnline.Dtos
{
    public class QuestionResponseDto
    {
        public int QuestionId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
    }
}
