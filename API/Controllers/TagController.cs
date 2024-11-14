using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlogApp.Models;
using BlogApp.Models.Services;
using API.Models;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Получает список всех тегов.
        /// </summary>
        /// <returns>Список всех тегов</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получает список всех тегов.")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            Logger.Info("Получен список всех тегов.");
            return Ok(tags);
        }

        /// <summary>
        /// Получает тег по ID.
        /// </summary>
        /// <param name="id">ID тега</param>
        /// <returns>Тег</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получает тег по ID.")]
        public async Task<ActionResult<Tag>> GetTag(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                Logger.Warn($"Тэг с ID {id} не найден.");
                return NotFound();
            }
            Logger.Info($"Получен тэг с ID {id}.");
            return Ok(tag);
        }

        /// <summary>
        /// Создает новый тег.
        /// </summary>
        /// <param name="dto">Модель данных для создания тега</param>
        /// <returns>Результат операции создания</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Создает новый тег.")]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = new Tag
            {
                Name = dto.Name
            };

            await _tagService.CreateTagAsync(tag);
            Logger.Info($"Тэг создан: {tag.Name}.");
            return CreatedAtAction(nameof(GetTag), new { id = tag.TagId }, tag);
        }

        /// <summary>
        /// Обновляет существующий тег.
        /// </summary>
        /// <param name="id">ID тега</param>
        /// <param name="dto">Модель данных для обновления тега</param>
        /// <returns>Результат операции обновления</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновляет существующий тег.")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] UpdateTagDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            tag.Name = dto.Name;

            try
            {
                await _tagService.UpdateTagAsync(tag);
                Logger.Info($"Тэг с ID {id} обновлен.");
            }
            catch (DbUpdateConcurrencyException)
            {
                Logger.Error($"Ошибка при обновлении тега с ID {id}. Возможны проблемы с синхронизацией данных.");
                if (!await TagExists(id))
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
        /// Удаляет тег по ID.
        /// </summary>
        /// <param name="id">ID тега</param>
        /// <returns>Результат операции удаления</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удаляет тег по ID.")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                Logger.Warn($"Тэг с ID {id} не найден для удаления.");
                return NotFound();
            }

            await _tagService.DeleteTagAsync(id);
            Logger.Info($"Тэг с ID {id} успешно удален.");
            return NoContent();
        }

        private async Task<bool> TagExists(int id)
        {
            return await _tagService.GetTagByIdAsync(id) != null;
        }
    }
}
