using BlogApp.Models.Services;
using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Threading.Tasks;
using System.Linq;

namespace BlogApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UserController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            Logger.Info("Показана страница со всеми пользователями.");
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                Logger.Warn($"Пользователь с ID {id} не найден.");
                return NotFound();
            }
            Logger.Info($"Переход на страницу просмотра пользователя с ID {id}.");
            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            Logger.Info("Переход на страницу создания нового пользователя.");
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,FirstName,LastName,Email,Phone,RegistrationDate,RoleId,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                await _userService.CreateUserAsync(user);
                Logger.Info($"Пользователь успешно создан. ID: {user.UserId}");
                return RedirectToAction("Users", "Home");
            }

            // Логирование ошибок модели для отладки
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Logger.Error($"Ошибка в поле {state.Key}: {error.ErrorMessage}");
                    }
                }
            }

            Logger.Warn("Ошибка при создании пользователя.");
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                Logger.Warn($"Пользователь с ID {id} не найден для редактирования.");
                return NotFound();
            }

            // Получаем список ролей
            var roles = await _roleService.GetAllRolesAsync();

            var viewModel = new UserEditViewModel
            {
                User = user,
                Roles = roles,
                SelectedRoleId = user.RoleId
            };

            Logger.Info($"Переход на страницу редактирования пользователя с ID {id}.");
            return View(viewModel);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.User.UserId)
            {
                Logger.Warn($"ID пользователя {id} не совпадает с ID в модели {model.User.UserId}. Обновление невозможно.");
                return NotFound();
            }

            // Проверка доступных ролей
            model.Roles = await _roleService.GetAllRolesAsync();
            var selectedRole = model.Roles.FirstOrDefault(r => r.RoleId == model.SelectedRoleId);

            if (selectedRole == null)
            {
                ModelState.AddModelError("SelectedRoleId", "Выбранная роль недействительна.");
                Logger.Warn($"Выбор роли недействителен. Выбранный RoleId: {model.SelectedRoleId}");
                return View(model);
            }

            try
            {
                model.User.RoleId = selectedRole.RoleId;
                model.User.Role = selectedRole;
                await _userService.UpdateUserAsync(model.User);
                Logger.Info($"Пользователь с ID {id} успешно обновлен.");
            }
            catch (DbUpdateConcurrencyException)
            {
                Logger.Error($"Ошибка при редактировании пользователя с ID {id}. Возможны проблемы с синхронизацией данных.");
                if (!await UserExistsAsync(model.User.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            model.Roles = await _roleService.GetAllRolesAsync();
            return RedirectToAction("Users", "Home");
        }

        private async Task<bool> UserExistsAsync(int id)
        {
            return await _userService.GetUserByIdAsync(id) != null;
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                Logger.Warn($"Пользователь с ID {id} не найден для удаления.");
                return NotFound();
            }

            Logger.Info($"Переход на страницу подтверждения удаления пользователя с ID {id}.");
            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userService.DeleteUserAsync(id);
            Logger.Info($"Пользователь с ID {id} успешно удален.");
            return RedirectToAction("Users", "Home");
        }
    }
}
