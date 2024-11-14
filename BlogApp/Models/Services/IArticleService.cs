namespace BlogApp.Models.Services
{
    public interface IArticleService
    {
        Task<IEnumerable<Article>> GetAllArticlesAsync();
        Task<Article> GetArticleByIdAsync(int id);
        Task<IEnumerable<Article>> GetArticlesByAuthorIdAsync(int authorId);
        Task<Article> CreateArticleAsync(Article article);
        Task<Article> UpdateArticleAsync(Article article);
        Task<bool> DeleteArticleAsync(int id);
    }
}
