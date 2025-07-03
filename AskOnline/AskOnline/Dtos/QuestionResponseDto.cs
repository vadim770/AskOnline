namespace AskOnline.Dtos
{
    public class QuestionResponseDto
    {
        public int QuestionId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public UserResponseDto User { get; set; } = new();
        public List<AnswerResponseDto> Answers { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
    }
}
