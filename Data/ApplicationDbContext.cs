using Microsoft.EntityFrameworkCore;
using PlayAndConnect.Models;
using Pomelo.EntityFrameworkCore.MySql;
using System.Reflection.Metadata;

namespace PlayAndConnect.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
              Database.EnsureCreated();
        }
        public DbSet<User> Users;
        public DbSet<Game> Games;
        public DbSet<Chat> Chats;
        public DbSet<Message> Messages;
        public DbSet<Like> Likes;
        public DbSet<Genre> Genres;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Chats)
                .WithMany(c => c.Users); 

            modelBuilder.Entity<User>()
                .HasMany(u => u.Likes)
                .WithMany(c => c.Users);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Games)
                .WithMany(c => c.Users);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Messages)
                .WithOne(c => c.User);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat);

            modelBuilder.Entity<Game>()
                .HasMany(g => g.Users)
                .WithMany(l => l.Games);

            modelBuilder.Entity<Game>()
                .HasMany(g => g.Genres)
                .WithMany(l => l.Games);
        }
    }
}
