public class AnswerResponseDto
{
    public int AnswerId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int QuestionId { get; set; }
    public int UserId { get; set; }
}
