using Microsoft.EntityFrameworkCore;

namespace BlogApp.Models.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;

        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return false;
            }
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
