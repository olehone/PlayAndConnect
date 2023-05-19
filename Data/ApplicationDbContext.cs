using Microsoft.EntityFrameworkCore;
using PlayAndConnect.Models;
using PlayAndConnect.Data.Configurations;
using Pomelo.EntityFrameworkCore.MySql;
using System.Reflection.Metadata;

namespace PlayAndConnect.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        public DbSet<User> Users => Set<User>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());    
        }
        /*
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<Genre> Genres => Set<Genre>();
        
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
        }*/
    }
}
