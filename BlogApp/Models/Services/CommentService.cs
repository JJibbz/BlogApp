using Microsoft.EntityFrameworkCore;

namespace BlogApp.Models.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _context.Comments
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == id);
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return false;
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
