using BlogApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApp
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связи многие ко многим между Article и Tag
            modelBuilder.Entity<Article>()
                .HasMany(a => a.Tags)
                .WithMany(t => t.Articles)
                .UsingEntity<Dictionary<string, object>>(
                    "ArticleTag",
                    j => j
                        .HasOne<Tag>()
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Article>()
                        .WithMany()
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("ArticleId", "TagId");
                    });

            // Настройка связи между User и Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка каскадного удаления для комментариев, связанных со статьей
            modelBuilder.Entity<Article>()
                .HasMany(a => a.Comments)
                .WithOne(c => c.Article)
                .HasForeignKey(c => c.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка каскадного удаления для комментариев, связанных с пользователем
            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
