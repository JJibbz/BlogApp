using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class ArticleViewModel
    {
        public ArticleViewModel()
        {
            AvailableTags = new List<TagViewModel>();
            SelectedTags = new List<int>();
        }

        public int ArticleId { get; set; }

        public int ViewCount { get; set; }

        [Required(ErrorMessage = "Введите заголовок")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Заполните контент")]
        public string Content { get; set; }

        public List<TagViewModel> AvailableTags { get; set; }

        [ValidateSelectedTags(ErrorMessage = "Должен быть выбран минимум 1 тэг")]
        public List<int> SelectedTags { get; set; }
    }

    public class TagViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public class ValidateSelectedTagsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var selectedTags = value as List<int>;
            if (selectedTags == null || !selectedTags.Any())
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
