namespace AskOnline.Dtos
{
    public class UserPublicDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
    }

    public class UserAdminDto : UserPublicDto
    {
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}