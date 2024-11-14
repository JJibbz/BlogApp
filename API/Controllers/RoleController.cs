using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlogApp.Models;
using BlogApp.Models.Services;
using API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Получает список всех ролей.
        /// </summary>
        /// <returns>Список всех ролей</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получает список всех ролей.")]
        public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            Logger.Info("Получен список всех ролей.");
            return Ok(roles);
        }

        /// <summary>
        /// Получает роль по ID.
        /// </summary>
        /// <param name="id">ID роли</param>
        /// <returns>Роль</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получает роль по ID.")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                Logger.Warn($"Роль с ID {id} не найдена.");
                return NotFound();
            }

            Logger.Info($"Получена роль с ID {id}: {role.Name}");
            return Ok(role);
        }

        /// <summary>
        /// Создает новую роль.
        /// </summary>
        /// <param name="dto">Модель данных для создания роли</param>
        /// <returns>Результат операции создания</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Создает новую роль.")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _roleService.CreateRoleAsync(role);

            Logger.Info($"Роль создана: {role.Name}");
            return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, role);
        }

        /// <summary>
        /// Обновляет существующую роль.
        /// </summary>
        /// <param name="id">ID роли</param>
        /// <param name="dto">Модель данных для обновления роли</param>
        /// <returns>Результат операции обновления</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновляет существующую роль.")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            role.Name = dto.Name;
            role.Description = dto.Description;

            await _roleService.UpdateRoleAsync(role);

            Logger.Info($"Роль с ID {id} обновлена. Новое имя: {role.Name}");
            return NoContent();
        }

        /// <summary>
        /// Удаляет роль по ID.
        /// </summary>
        /// <param name="id">ID роли</param>
        /// <returns>Результат операции удаления</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удаляет роль по ID.")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
            {
                Logger.Warn($"Роль с ID {id} не найдена для удаления.");
                return NotFound();
            }

            Logger.Info($"Роль с ID {id} успешно удалена.");
            return NoContent();
        }
    }
}
