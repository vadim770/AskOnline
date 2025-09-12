namespace AskOnline.Dtos
{
    public class QuestionUpdateDto
    {
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public List<string> Tags { get; set; } = new();
    }
}
