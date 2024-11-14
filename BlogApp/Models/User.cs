using BlogApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите номер телефона")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(4, ErrorMessage = "Пароль должен состоять минимум из 4х символов")]
        public string Password { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public int RoleId { get; set; }

        public Role Role { get; set; }
        public ICollection<Article> Articles { get; set; } = new List<Article>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
