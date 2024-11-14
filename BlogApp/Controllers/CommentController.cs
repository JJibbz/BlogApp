using BlogApp.Models.Services;
using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using NLog;

namespace BlogApp.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly ApplicationDbContext _context;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CommentController(ICommentService commentService, ApplicationDbContext context)
        {
            _commentService = commentService;
            _context = context;
        }

        // GET: Comment
        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            var comments = await _context.Comments.ToListAsync();
            if (comments == null)
            {
                Logger.Warn("Список комментариев пуст или не найден.");
                comments = new List<Comment>();
            }
            return comments;
        }

        // GET: Comment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                Logger.Warn($"Комментарий с ID: {id} не найден.");
                return NotFound();
            }

            Logger.Info($"Показаны детали комментария с ID: {id}");
            return View(comment);
        }

        // GET: Comment/Create
        public IActionResult Create(int articleId)
        {
            // Передаем ID статьи в представление, чтобы знать, к какой статье добавлять комментарий
            ViewBag.ArticleId = articleId;
            Logger.Info($"Переход на страницу создания комментария для статьи с ID: {articleId}");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int articleId, string content)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Logger.Warn("Неавторизованный пользователь пытается добавить комментарий.");
                return Unauthorized();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                Logger.Warn("Не удалось получить ID пользователя из утверждений.");
                return BadRequest("Invalid User ID format");
            }

            var article = await _context.Articles.FindAsync(articleId);
            if (article == null)
            {
                Logger.Warn($"Статья с ID: {articleId} не найдена.");
                return NotFound("Article not found");
            }

            var comment = new Comment
            {
                ArticleId = articleId,
                UserId = userId,
                Content = content,
                CommentDate = DateTime.UtcNow
            };

            Logger.Info($"Добавлен комментарий к статье с ID: {articleId} пользователем с ID: {userId}");
            await _commentService.CreateCommentAsync(comment);

            TempData["IncreaseViewCount"] = false;

            // Перенаправление на страницу статьи, но не вызываем метод Details
            return RedirectToAction("Details", "Article", new { id = articleId, isView = false });
        }

        // GET: Comment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                Logger.Warn($"Комментарий с ID: {id} не найден при попытке редактирования.");
                return NotFound();
            }

            Logger.Info($"Переход на страницу редактирования комментария с ID: {id}");
            return View(comment);
        }

        // POST: Comment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,ArticleId,UserId,Content,CommentDate")] Comment comment)
        {
            if (id != comment.CommentId)
            {
                Logger.Warn($"ID комментария в URL и в модели не совпадают. URL ID: {id}, Модель ID: {comment.CommentId}");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Logger.Info($"Попытка обновления комментария с ID: {id}");
                    await _commentService.UpdateCommentAsync(comment);
                    Logger.Info($"Комментарий с ID: {id} успешно обновлен.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    Logger.Error($"Ошибка при обновлении комментария с ID: {id}. Конкуренция обновлений.");
                    if (!await CommentExists(comment.CommentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Comments", "Home");
            }

            Logger.Warn($"Ошибка при обновлении комментария с ID: {id}. Некорректное состояние модели.");
            return View(comment);
        }

        // GET: Comment/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                Logger.Warn($"Комментарий с ID: {id} не найден при попытке удаления.");
                return NotFound();
            }

            Logger.Info($"Переход на страницу подтверждения удаления комментария с ID: {id}");
            return View(comment);
        }

        // POST: Comment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Logger.Info($"Удаление комментария с ID: {id}");
            await _commentService.DeleteCommentAsync(id);
            return RedirectToAction("Comments", "Home");
        }

        // Проверка существования комментария
        private async Task<bool> CommentExists(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            return comment != null;
        }
    }
}
