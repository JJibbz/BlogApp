using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CreateTagDto
    {
        [Required(ErrorMessage = "Введите имя тега")]
        public string Name { get; set; }
    }
}
