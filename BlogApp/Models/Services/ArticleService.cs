using Microsoft.EntityFrameworkCore;

namespace BlogApp.Models.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;

        public ArticleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Tags) 
                .ToListAsync();
        }

        public async Task<Article> GetArticleByIdAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.User)
                .Include(a => a.Comments)
                    .ThenInclude(c => c.User)
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
        }

        public async Task<IEnumerable<Article>> GetArticlesByAuthorIdAsync(int authorId)
        {
            return await _context.Articles.Where(a => a.UserId == authorId).ToListAsync();
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article> UpdateArticleAsync(Article article)
        {
            _context.Entry(article).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return false;
            }
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
