namespace AskOnline.Dtos
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public DateTime? CreatedAt { get; set; }

        // only populated for the owner
        public string? Email { get; set; }
        public string Role { get; set; } = "User";
    }
}
