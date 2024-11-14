using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Tag
    {
        public int TagId { get; set; }

        [Required(ErrorMessage = "Введите имя роли")]
        public string Name { get; set; }

        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
