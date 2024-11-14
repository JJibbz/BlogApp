namespace BlogApp.Models
{
    public class Article
    {
        public int ArticleId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublicationDate { get; set; } = DateTime.UtcNow;
        public int ViewCount { get; set; }

        public User User { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}
