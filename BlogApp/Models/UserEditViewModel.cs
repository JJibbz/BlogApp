namespace BlogApp.Models
{
    public class UserEditViewModel
    {
        public User User { get; set; }
        public IEnumerable<Role> Roles { get; set; } = Enumerable.Empty<Role>(); // Инициализация по умолчанию
        public int SelectedRoleId { get; set; }
    }
}
