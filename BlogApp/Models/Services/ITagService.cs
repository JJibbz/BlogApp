namespace BlogApp.Models.Services
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag> GetTagByIdAsync(int id);
        Task<Tag> CreateTagAsync(Tag tag);
        Task<Tag> UpdateTagAsync(Tag tag);
        Task<bool> DeleteTagAsync(int id);
    }
}
