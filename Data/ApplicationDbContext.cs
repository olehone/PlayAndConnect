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
            Database.EnsureDeleted();
            Database.EnsureCreated();

        }
        public DbSet<User> Users => Set<User>();
        public DbSet<UserInfo> Infos => Set<UserInfo>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<Message> Messages => Set<Message>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //User
            modelBuilder.Entity<User>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<User>().Property(p => p.Login).HasMaxLength(30).IsRequired();
            modelBuilder.Entity<User>().HasOne(u => u.Info).WithOne(i => i.User).HasForeignKey<UserInfo>(x => x.UserId).HasPrincipalKey<User>(u => u.Id);
            modelBuilder.Entity<User>().HasMany(u => u.Games).WithMany(g => g.Users);
            modelBuilder.Entity<User>().HasMany(u => u.Chats).WithMany(g => g.Users);

            //UserInfo
            modelBuilder.Entity<UserInfo>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UserInfo>().Property(p => p.ImagePath).IsRequired(false);

            //Game
            modelBuilder.Entity<Game>().Property(g => g.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Game>().Property(g => g.Description).IsRequired(false);
            modelBuilder.Entity<Game>().Property(g => g.Title).IsRequired(false);
            modelBuilder.Entity<Game>().HasOne(g => g.Genre).WithMany(g => g.Games);

            //Genre
            modelBuilder.Entity<Genre>().Property(g => g.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Genre>().Property(g => g.Name).IsRequired();

            //Like
            modelBuilder.Entity<Like>().Property(l => l.Id).ValueGeneratedOnAdd();

            //Chat
            modelBuilder.Entity<Chat>().Property(l => l.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Chat>().HasMany(c => c.Messages).WithOne(c => c.Chat);

            //Message
            modelBuilder.Entity<Chat>().Property(l => l.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Message>().HasOne(c => c.User).WithMany(c => c.Messages);
        }
        /*
        public DbSet<Like> Likes => Set<Like>();
        
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


        }*/
    }
}
