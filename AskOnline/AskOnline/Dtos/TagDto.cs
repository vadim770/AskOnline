namespace AskOnline.Dtos
{
    public class TagDto
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TagCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AddTagToQuestionRequestDto
    {
        public int QuestionId { get; set; }
        public string TagName { get; set; } = string.Empty;
    }

}