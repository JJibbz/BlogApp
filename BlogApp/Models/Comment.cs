using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int ArticleId { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Введите комментарий")]
        public string Content { get; set; }
        public DateTime CommentDate { get; set; } = DateTime.UtcNow;

        public Article Article { get; set; }
        public User User { get; set; }
    }
}
