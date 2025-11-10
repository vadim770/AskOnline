using System.ComponentModel.DataAnnotations;

namespace AskOnline.Dtos
{
    public class AnswerRequestDto
    {
        public int QuestionId { get; set; }
        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "the answer body must be between 1 and 500 characters.")]
        public string Body { get; set; } = string.Empty;
    }
}
