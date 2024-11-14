using API.Models;
using BlogApp.Models;
using BlogApp.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly ITagService _tagService;
        private readonly IUserService _userService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _context;

        public ArticlesController(IArticleService articleService, IUserService userService, ITagService tagService, ApplicationDbContext context)
        {
            _articleService = articleService;
            _userService = userService;
            _tagService = tagService;
            _context = context;
        }

        /// <summary>
        /// Получает список всех статей.
        /// </summary>
        /// <returns>Список статей</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Получает список всех статей.",
            Description = "Возвращает список всех статей из базы данных.",
            OperationId = "GetAllArticles"
        )]
        public async Task<ActionResult<IEnumerable<Article>>> GetAllArticles()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            Logger.Info("Получен список всех статей.");
            return Ok(articles);
        }

        /// <summary>
        /// Получает статью по ID.
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <returns>Статья</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Получает статью по ID.",
            Description = "Возвращает статью, соответствующую указанному ID.",
            OperationId = "GetArticle"
        )]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                Logger.Warn($"Статья с ID {id} не найдена.");
                return NotFound();
            }

            Logger.Info($"Получена статья с ID {id}: {article.Title}");
            return Ok(article);
        }

        /// <summary>
        /// Создает новую статью.
        /// </summary>
        /// <param name="dto">Модель данных для создания статьи</param>
        /// <returns>Результат операции создания</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Создает новую статью.",
            Description = "Создает новую статью в базе данных и возвращает созданную статью.",
            OperationId = "CreateArticle"
        )]
        public async Task<IActionResult> CreateArticle([FromBody] CreateArticleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = new Article
            {
                Title = dto.Title,
                Content = dto.Content,
                UserId = 2
            };

            if (dto.SelectedTags != null && dto.SelectedTags.Any())
            {
                var tags = await _context.Tags.Where(tag => dto.SelectedTags.Contains(tag.TagId)).ToListAsync();
                article.Tags = tags;
            }

            await _articleService.CreateArticleAsync(article);

            return CreatedAtAction(nameof(CreateArticle), new { id = article.ArticleId }, article);
        }

        /// <summary>
        /// Обновляет существующую статью.
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <param name="model">Модель данных для обновления статьи</param>
        /// <returns>Результат операции обновления</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Обновляет существующую статью.",
            Description = "Обновляет статью по указанному ID и возвращает статус обновления.",
            OperationId = "UpdateArticle"
        )]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] UpdateArticleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            article.Title = model.Title;
            article.Content = model.Content;

            if (model.SelectedTags != null && model.SelectedTags.Any())
            {
                var tags = await _context.Tags.Where(tag => model.SelectedTags.Contains(tag.TagId)).ToListAsync();
                article.Tags = tags;
            }
            else
            {
                article.Tags = new List<Tag>();
            }

            await _articleService.UpdateArticleAsync(article);

            return NoContent();
        }

        /// <summary>
        /// Удаляет статью по ID.
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <returns>Результат операции удаления</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Удаляет статью по ID.",
            Description = "Удаляет статью с указанным ID из базы данных.",
            OperationId = "DeleteArticle"
        )]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var result = await _articleService.DeleteArticleAsync(id);
            if (!result)
            {
                Logger.Warn($"Статья с ID {id} не найдена для удаления.");
                return NotFound();
            }

            Logger.Info($"Статья с ID {id} успешно удалена.");
            return NoContent();
        }
    }
}
