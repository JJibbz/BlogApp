using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UpdateTagDto
    {
        [Required(ErrorMessage = "Введите имя тега")]
        public string Name { get; set; }
    }
}
