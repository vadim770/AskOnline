using System.ComponentModel.DataAnnotations;

namespace AskOnline.Dtos
{
    public class QuestionRequestDto
    {
        [Required]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "the title must be between 1 and 30 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "the body must be between 1 and 500 characters.")]
        public string Body { get; set; } = string.Empty;

        public List<string> TagNames { get; set; } = new();
    }

}
