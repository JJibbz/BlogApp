using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UpdateCommentDto
    {
        [Required(ErrorMessage = "Укажите текст комментария")]
        public string Content { get; set; }
    }
}
