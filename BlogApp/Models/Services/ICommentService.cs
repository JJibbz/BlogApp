namespace BlogApp.Models.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<Comment> GetCommentByIdAsync(int id);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<Comment> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int id);
    }
}
