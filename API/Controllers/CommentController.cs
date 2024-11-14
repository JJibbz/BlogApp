using API.Models;
using BlogApp.Models;
using BlogApp.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentApiController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ApplicationDbContext _context;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CommentApiController(ICommentService commentService, ApplicationDbContext context)
        {
            _commentService = commentService;
            _context = context;
        }

        /// <summary>
        /// Получает список всех комментариев.
        /// </summary>
        /// <returns>Список всех комментариев</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получает список всех комментариев.")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetAllComments()
        {
            var comments = await _context.Comments.ToListAsync();
            if (comments == null || !comments.Any())
            {
                Logger.Warn("Список комментариев пуст или не найден.");
                return NotFound("No comments found.");
            }

            Logger.Info("Получен список всех комментариев.");
            return Ok(comments);
        }

        /// <summary>
        /// Получает комментарий по ID.
        /// </summary>
        /// <param name="id">ID комментария</param>
        /// <returns>Комментарий</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получает комментарий по ID.")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                Logger.Warn($"Комментарий с ID {id} не найден.");
                return NotFound();
            }

            Logger.Info($"Получен комментарий с ID {id}: {comment.Content}");
            return Ok(comment);
        }

        /// <summary>
        /// Создает новый комментарий.
        /// </summary>
        /// <param name="dto">Модель данных для создания комментария</param>
        /// <returns>Результат операции создания</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Создает новый комментарий.")]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = new Comment
            {
                ArticleId = dto.ArticleId,
                UserId = 2,
                Content = dto.Content,
                CommentDate = DateTime.UtcNow
            };

            try
            {
                await _commentService.CreateCommentAsync(comment);
                Logger.Info($"Комментарий добавлен для статьи с ID: {dto.ArticleId}");
                return CreatedAtAction(nameof(GetComment), new { id = comment.CommentId }, comment);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка при создании комментария");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновляет существующий комментарий.
        /// </summary>
        /// <param name="id">ID комментария</param>
        /// <param name="dto">Модель данных для обновления комментария</param>
        /// <returns>Результат операции обновления</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновляет существующий комментарий.")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment == null)
            {
                Logger.Warn($"Комментарий с ID: {id} не найден.");
                return NotFound();
            }

            existingComment.Content = dto.Content;

            try
            {
                await _commentService.UpdateCommentAsync(existingComment);
                Logger.Info($"Комментарий с ID: {id} успешно обновлен.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                Logger.Error($"Ошибка при обновлении комментария с ID: {id}. Конкуренция обновлений.");
                if (!await CommentExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка при обновлении комментария");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удаляет комментарий по ID.
        /// </summary>
        /// <param name="id">ID комментария</param>
        /// <returns>Результат операции удаления</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удаляет комментарий по ID.")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var existingComment = await _commentService.GetCommentByIdAsync(id);
            if (existingComment == null)
            {
                Logger.Warn($"Комментарий с ID: {id} не найден.");
                return NotFound();
            }

            try
            {
                await _commentService.DeleteCommentAsync(id);
                Logger.Info($"Комментарий с ID: {id} удален.");
                return NoContent();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка при удалении комментария");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        private async Task<bool> CommentExists(int id)
        {
            return await _commentService.GetCommentByIdAsync(id) != null;
        }
    }
}
