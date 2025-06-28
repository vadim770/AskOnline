namespace AskOnline.Dtos
{
    public class QuestionRequestDto
    {
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public List<string> TagNames { get; set; } = new();
    }
}
