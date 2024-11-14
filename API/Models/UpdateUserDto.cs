using System.ComponentModel.DataAnnotations;

namespace BlogApp.API.Models
{
    public class UpdateUserDto
    {
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

        public int RoleId { get; set; }
    }
}
