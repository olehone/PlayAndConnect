using PlayAndConnect.Models;
using PlayAndConnect.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace PlayAndConnect.Data.Interfaces
{
    public class UserDb : IUserDb
    {
        private readonly ApplicationDbContext _db;
        public UserDb(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<bool> Create(User entity)
        {
            try
            {
                await _db.Users.AddAsync(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception! Message: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Edit(User entity, int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }
            user = entity;
            user.Id = id;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<User?> Get(int id)
        {
            User? user = await _db.Users.FindAsync(id);
            return user;
        }
        public ICollection<Chat> GetChats(User user)
        {
            return user.Chats;
        }

        public ICollection<Game> GetGames(User user)
        {
            return user.Games;
        }

        public ICollection<Like> GetLikes(User user)
        {
            return user.Likes;
        }

        public ICollection<Message> GetMessages(User user, Chat chat)
        {
            return user.Messages;
        }
        public async Task<User?> GetByName(string name)
        {
            User? user = await _db.Users.FirstOrDefaultAsync(u => u.Name == name);
            return user;
        }
        public async Task<bool> Verify(string name, string password)
        {
            User? user = await GetByName(name);
            if (user == null)
                return false;
            else if (user.PasswordHash == PasswordHasher.HashPassword(password))
                return true;
            else return false;
        }

    }
}