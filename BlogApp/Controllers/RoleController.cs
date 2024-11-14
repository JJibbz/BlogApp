using BlogApp.Models.Services;
using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: Role
        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetAllRolesAsync();
            Logger.Info("Показана страница со всеми ролями.");
            return View(roles);
        }

        // GET: Role/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                Logger.Warn($"Роль с ID {id} не найдена.");
                return NotFound();
            }
            Logger.Info($"Переход на страницу просмотра роли с ID {id}.");
            return View(role);
        }

        // GET: Role/Create
        public IActionResult Create()
        {
            Logger.Info("Переход на страницу создания новой роли.");
            return View();
        }

        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleId,Name,Description")] Role role)
        {
            if (ModelState.IsValid)
            {
                await _roleService.CreateRoleAsync(role);
                Logger.Info($"Роль успешно создана. Название: {role.Name}.");
                return RedirectToAction("Roles", "Home");
            }

            // Вывод ошибок модели для отладки
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

            Logger.Warn("Ошибка при создании роли.");
            return View(role);
        }

        // GET: Role/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                Logger.Warn($"Роль с ID {id} не найдена для редактирования.");
                return NotFound();
            }
            Logger.Info($"Переход на страницу редактирования роли с ID {id}.");
            return View(role);
        }

        // POST: Role/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoleId,Name,Description")] Role role)
        {
            if (id != role.RoleId)
            {
                Logger.Warn($"ID роли {id} не совпадает с ID в модели {role.RoleId}. Обновление невозможно.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _roleService.UpdateRoleAsync(role);
                    Logger.Info($"Роль с ID {id} успешно отредактирована. Новое название: {role.Name}.");
                    return RedirectToAction("Roles", "Home");
                }
                catch (DbUpdateConcurrencyException)
                {
                    Logger.Error($"Ошибка при редактировании роли с ID {id}. Возможны проблемы с синхронизацией данных.");
                    if (!await RoleExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            Logger.Warn($"Ошибка при редактировании роли с ID {id}. Проверьте состояние модели.");
            return View(role);
        }

        // GET: Role/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                Logger.Warn($"Роль с ID {id} не найдена для удаления.");
                return NotFound();
            }

            Logger.Info($"Переход на страницу подтверждения удаления роли с ID {id}.");
            return View(role);
        }

        // POST: Role/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _roleService.DeleteRoleAsync(id);
            Logger.Info($"Роль с ID {id} успешно удалена.");
            return RedirectToAction("Roles", "Home");
        }

        private async Task<bool> RoleExists(int id)
        {
            return await _roleService.GetRoleByIdAsync(id) != null;
        }
    }
}
