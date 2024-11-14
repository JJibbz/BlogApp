using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "Укажите текст комментария")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Укажите ID статьи")]
        public int ArticleId { get; set; }
    }
}
