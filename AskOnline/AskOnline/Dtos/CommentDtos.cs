namespace AskOnline.Dtos
{
    public class CommentCreateDto
    {
        public string Text { get; set; }
    }

    public class CommentDto
    {
        public int CommentId { get; set; }
        public string Text { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CommentResponseDto
    {
        public int CommentId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserResponseDto User { get; set; } = new();
    }

    public class CommentUpdateDto
    {
        public string Text { get; set; } = string.Empty;
    }
}
