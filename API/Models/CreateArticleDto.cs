using BlogApp.Models;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CreateArticleDto
    {
        [Required(ErrorMessage = "Введите заголовок")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Заполните контент")]
        public string Content { get; set; }

        [ValidateSelectedTags(ErrorMessage = "Должен быть выбран минимум 1 тэг")]
        public List<int> SelectedTags { get; set; }
    }
}
