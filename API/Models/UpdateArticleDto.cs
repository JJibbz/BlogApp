using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UpdateArticleDto
    {
        [Required(ErrorMessage = "Введите заголовок")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Заполните контент")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать хотя бы один тэг")]
        public List<int> SelectedTags { get; set; }
    }

}
