using BlogApp.Models.Services;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using NLog;
using System.Linq;

namespace BlogApp.Controllers
{
    [Authorize]
    public class ArticleController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly IUserService _userService;
        private readonly ITagService _tagService;
        private readonly ApplicationDbContext _context;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ArticleController(IArticleService articleService, IUserService userService, ITagService tagService, ApplicationDbContext context)
        {
            _articleService = articleService;
            _userService = userService;
            _tagService = tagService;
            _context = context;
        }

        // GET: Article/UserArticles
        public async Task<IActionResult> UserArticles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                Logger.Warn("Неуспешная попытка перехода на страницу статей пользователя: идентификатор пользователя не найден.");
                return Unauthorized();
            }

            Logger.Info($"Успешная попытка перехода на страницу статей пользователя. UserId: {userId}");
            var articles = await _articleService.GetArticlesByAuthorIdAsync(int.Parse(userId));
            return View(articles);
        }

        // GET: Article
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            Logger.Info($"Переход на главную страницу статей. UserId: {userId}, Role: {role}");
            var articles = await _articleService.GetAllArticlesAsync();
            return View(articles);
        }

        // GET: Article/Details/5
        public async Task<IActionResult> Details(int id, bool isView = true)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                Logger.Warn($"Статья с ID {id} не найдена.");
                return NotFound();
            }

            if (isView)
            {
                article.ViewCount++;
                Logger.Info($"Счетчик просмотров статьи с ID {id} увеличен. Новый счетчик: {article.ViewCount}");
                await _articleService.UpdateArticleAsync(article);
            }

            Logger.Info($"Переход на страницу статьи с ID {id}. Статья: {article.Title}");
            return View(article);
        }

        // GET: Article/Create
        public async Task<IActionResult> Create()
        {
            var tags = await _tagService.GetAllTagsAsync();
            var model = new ArticleViewModel
            {
                AvailableTags = tags.Select(tag => new TagViewModel
                {
                    Id = tag.TagId,
                    Name = tag.Name
                }).ToList()
            };
            Logger.Info("Переход на страницу создания новой статьи.");
            return View(model);
        }

        // POST: Article/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Logger.Warn("Попытка создания статьи без авторизации.");
                return Unauthorized();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                Logger.Warn("Не удалось найти идентификатор пользователя в claims.");
                return BadRequest("User ID not found in claims");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                Logger.Warn("Формат идентификатора пользователя неверен.");
                return BadRequest("Invalid User ID format");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                Logger.Warn($"Пользователь с ID {userId} не найден.");
                return NotFound("User not found");
            }

            if (ModelState.IsValid)
            {
                var article = new Article
                {
                    Title = model.Title,
                    Content = model.Content,
                    UserId = user.UserId
                };

                if (model.SelectedTags != null && model.SelectedTags.Any())
                {
                    var _tags = await _context.Tags.Where(tag => model.SelectedTags.Contains(tag.TagId)).ToListAsync();
                    article.Tags = _tags;
                }

                Logger.Info($"Статья успешно создана. UserId: {user.UserId}, Title: {model.Title}");

                await _articleService.CreateArticleAsync(article);
                return RedirectToAction("Articles", "Home");
            }

            var tags = await _tagService.GetAllTagsAsync();
            model.AvailableTags = tags.Select(tag => new TagViewModel
            {
                Id = tag.TagId,
                Name = tag.Name
            }).ToList();

            Logger.Warn($"Ошибка при создании статьи. UserId: {user.UserId}, Title: {model.Title}");

            return View(model);
        }

        // GET: Article/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                Logger.Warn($"Статья с ID {id} не найдена для редактирования.");
                return NotFound();
            }

            var tags = await _tagService.GetAllTagsAsync();
            if (tags == null)
            {
                Logger.Warn("Тэги не найдены при редактировании статьи, создан пустой список.");
                tags = new List<Tag>(); // Предотвращаем ошибку, если tags равен null
            }

            var model = new ArticleViewModel
            {
                ArticleId = id,
                Title = article.Title,
                Content = article.Content,
                SelectedTags = article.Tags?.Select(t => t.TagId).ToList() ?? new List<int>(),
                AvailableTags = tags.Select(tag => new TagViewModel
                {
                    Id = tag.TagId,
                    Name = tag.Name,
                    IsSelected = article.Tags?.Any(t => t.TagId == tag.TagId) ?? false
                }).ToList()
            };

            Logger.Info($"Переход на страницу редактирования статьи с ID {id}.");
            return View(model);
        }

        // POST: Article/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArticleViewModel model)
        {
            if (id != model.ArticleId)
            {
                Logger.Warn($"Неверный ID статьи при редактировании. Ожидался {model.ArticleId}, получен {id}.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingArticle = await _articleService.GetArticleByIdAsync(id);
                    if (existingArticle == null)
                    {
                        Logger.Warn($"Статья с ID {id} не найдена для обновления.");
                        return NotFound();
                    }

                    existingArticle.Title = model.Title;
                    existingArticle.Content = model.Content;

                    var selectedTags = model.SelectedTags ?? new List<int>();
                    var tags = await _context.Tags.Where(t => selectedTags.Contains(t.TagId)).ToListAsync();

                    existingArticle.Tags = tags;

                    Logger.Info($"Статья с ID {id} успешно обновлена. Новый заголовок: {model.Title}");

                    await _articleService.UpdateArticleAsync(existingArticle);
                }
                catch (DbUpdateConcurrencyException)
                {
                    Logger.Error($"Ошибка при редактировании статьи с ID {id}. Возможны проблемы с синхронизацией данных.");
                    if (!await ArticleExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Articles", "Home");
            }

            var allTags = await _tagService.GetAllTagsAsync();
            model.AvailableTags = allTags.Select(tag => new TagViewModel
            {
                Id = tag.TagId,
                Name = tag.Name,
                IsSelected = model.SelectedTags.Contains(tag.TagId)
            }).ToList();

            return View(model);
        }

        // GET: Article/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                Logger.Warn($"Статья с ID {id} не найдена для удаления.");
                return NotFound();
            }

            Logger.Info($"Переход на страницу подтверждения удаления статьи с ID {id}.");
            return RedirectToAction("DeleteConfirmed", new { id });
        }

        // POST: Article/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _articleService.DeleteArticleAsync(id);
            Logger.Info($"Статья с ID {id} успешно удалена.");
            return RedirectToAction("Articles", "Home");
        }

        private async Task<bool> ArticleExists(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            return article != null;
        }
    }
}
