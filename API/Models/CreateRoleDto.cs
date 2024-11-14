using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "Введите имя роли")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите описание роли")]
        public string Description { get; set; }
    }
}
