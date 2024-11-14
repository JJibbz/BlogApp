using BlogApp.Models.Services;
using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: Tag
        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAllTagsAsync();
            Logger.Info("Показана страница со всеми тэгами.");
            return View(tags);
        }

        // GET: Tag/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                Logger.Warn($"Тэг с ID {id} не найден.");
                return NotFound();
            }
            Logger.Info($"Переход на страницу просмотра тэга с ID {id}.");
            return View(tag);
        }

        // GET: Tag/Create
        public IActionResult Create()
        {
            Logger.Info("Переход на страницу создания нового тега.");
            return View();
        }

        // POST: Tag/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TagId,Name")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                await _tagService.CreateTagAsync(tag);
                Logger.Info($"Тэг успешно создан. Название: {tag.Name}.");
                return RedirectToAction("Tags", "Home");
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

            Logger.Warn("Ошибка при создании тега.");
            return View(tag);
        }

        // GET: Tag/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                Logger.Warn($"Тэг с ID {id} не найден для редактирования.");
                return NotFound();
            }
            Logger.Info($"Переход на страницу редактирования тега с ID {id}.");
            return View(tag);
        }

        // POST: Tag/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TagId,Name")] Tag tag)
        {
            if (id != tag.TagId)
            {
                Logger.Warn($"ID тега {id} не совпадает с ID в модели {tag.TagId}. Обновление невозможно.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _tagService.UpdateTagAsync(tag);
                    Logger.Info($"Тэг с ID {id} успешно отредактирован. Новое название: {tag.Name}.");
                    return RedirectToAction("Tags", "Home");
                }
                catch (DbUpdateConcurrencyException)
                {
                    Logger.Error($"Ошибка при редактировании тега с ID {id}. Возможны проблемы с синхронизацией данных.");
                    if (!await TagExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            Logger.Warn($"Ошибка при редактировании тега с ID {id}. Проверьте состояние модели.");
            return View(tag);
        }

        // GET: Tag/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                Logger.Warn($"Тэг с ID {id} не найден для удаления.");
                return NotFound();
            }

            Logger.Info($"Переход на страницу подтверждения удаления тега с ID {id}.");
            return View(tag);
        }

        // POST: Tag/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _tagService.DeleteTagAsync(id);
            Logger.Info($"Тэг с ID {id} успешно удален.");
            return RedirectToAction("Tags", "Home");
        }

        private async Task<bool> TagExists(int id)
        {
            return await _tagService.GetTagByIdAsync(id) != null;
        }
    }
}
