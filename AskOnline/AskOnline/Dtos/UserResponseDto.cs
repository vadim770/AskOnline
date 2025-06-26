namespace AskOnline.Dtos
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";

        // only populated for the owner
        public string? Email { get; set; }
        public string Role { get; set; } = "User";
        public DateTime? CreatedAt { get; set; }
    }
}
