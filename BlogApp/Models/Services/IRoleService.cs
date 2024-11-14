namespace BlogApp.Models.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(int id);
        Task<Role> CreateRoleAsync(Role tag);
        Task<Role> UpdateRoleAsync(Role tag);
        Task<bool> DeleteRoleAsync(int id);
    }
}
