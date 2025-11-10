using System.ComponentModel.DataAnnotations;

namespace AskOnline.Dtos
{
    public class QuestionUpdateDto
    {
        [Required]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 30 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Body must be between 1 and 500 characters.")]
        public string Body { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }
}
