using BlogApp.API.Models;
using BlogApp.Models;
using BlogApp.Models.Services;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UsersController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        /// <summary>
        /// Получает список всех пользователей.
        /// </summary>
        /// <returns>Список пользователей</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получает список всех пользователей.")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            Logger.Info("Получен список всех пользователей.");
            return Ok(users);
        }

        /// <summary>
        /// Получает пользователя по ID.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>Пользователь</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получает пользователя по ID.")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                Logger.Warn($"Пользователь с ID {id} не найден.");
                return NotFound();
            }
            Logger.Info($"Получен пользователь с ID {id}.");
            return Ok(user);
        }

        /// <summary>
        /// Создает нового пользователя.
        /// </summary>
        /// <param name="dto">DTO для создания пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Создает нового пользователя.")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Password = dto.Password,
                RoleId = dto.RoleId
            };

            await _userService.CreateUserAsync(user);
            Logger.Info($"Пользователь создан. ID: {user.UserId}");
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        /// <summary>
        /// Обновляет информацию о пользователе.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="dto">DTO для обновления пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновляет информацию о пользователе.")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Password = dto.Password;
            user.RoleId = dto.RoleId;

            try
            {
                await _userService.UpdateUserAsync(user);
                Logger.Info($"Пользователь с ID {id} обновлен.");
            }
            catch (DbUpdateConcurrencyException)
            {
                Logger.Error($"Ошибка при обновлении пользователя с ID {id}. Возможны проблемы с синхронизацией данных.");
                if (!await UserExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Удаляет пользователя по ID.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удаляет пользователя по ID.")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                Logger.Warn($"Пользователь с ID {id} не найден для удаления.");
                return NotFound();
            }

            await _userService.DeleteUserAsync(id);
            Logger.Info($"Пользователь с ID {id} успешно удален.");
            return NoContent();
        }

        private async Task<bool> UserExistsAsync(int id)
        {
            return await _userService.GetUserByIdAsync(id) != null;
        }
    }
}
