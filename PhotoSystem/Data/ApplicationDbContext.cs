using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotoSystem.Models;

namespace PhotoSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PhotoPost> PhotoPosts { get; set; } = null!;
        public DbSet<PhotoLike> PhotoLikes { get; set; } = null!;
        public DbSet<Follow> Follows { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostInteraction> PostInteractions { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<StoryView> StoryViews { get; set; }
      
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Налаштування для системи підписок
            builder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Follow>()
                .HasIndex(f => new { f.FollowerId, f.FollowingId })
                .IsUnique();

            //Налаштування для PostInteraction
            builder.Entity<PostInteraction>()
                .HasOne(pi => pi.OriginalAuthor)
                .WithMany()
                .HasForeignKey(pi => pi.OriginalAuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PostInteraction>()
                .HasOne(pi => pi.Actor)
                .WithMany()
                .HasForeignKey(pi => pi.ActorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostInteraction>()
                .HasOne(pi => pi.OriginalPost)
                .WithMany()
                .HasForeignKey(pi => pi.OriginalPostId)
                .OnDelete(DeleteBehavior.Cascade);

            //Налаштування для Stories 
            builder.Entity<StoryView>()
                .HasOne(sv => sv.Story)
                .WithMany(s => s.Views)
                .HasForeignKey(sv => sv.StoryId)
                .OnDelete(DeleteBehavior.Cascade); // Видаляє історію -> видаляємо записи про перегляди

            builder.Entity<StoryView>()
                .HasOne(sv => sv.Viewer)
                .WithMany()
                .HasForeignKey(sv => sv.ViewerId)
                .OnDelete(DeleteBehavior.NoAction); // Уникає циклу каскадного видалення
        }
    }
}