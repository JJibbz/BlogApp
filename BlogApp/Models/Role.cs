using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Введите имя тэга")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите описание")]
        public string Description { get; set; }


        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
