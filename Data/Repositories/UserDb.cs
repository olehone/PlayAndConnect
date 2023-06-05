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
        public async Task<User?> GetUserByPassword(string login, string password)
        {
            return await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == login && u.PasswordHash == Hashing.HashPassword(password));
        }
        public async Task<User?> GetUserByLogin(string? login)
        {
            if (login != null)
                return await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == login);
            else
                return null;
        }
        public async Task<User?> GetUserById(int? id)
        {
            if (id != null)
                return await _db.Users.FirstOrDefaultAsync<User>(u => u.Id == id);
            else
                return null;
        }
        public async Task<User?> Create(string login, string password)
        {
            User? user = new User { Login = login, PasswordHash = Hashing.HashPassword(password) };
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return user;
        }
        public async Task<ICollection<User>?> GetUsersListWithGame(Game? game)
        {
            if(game!=null)
            {
                return await _db.Users.Where<User>(u => u.Games.Contains(game)).ToListAsync();
            }
            else return null;
        }





        /*
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
        /*
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
            else if (user.PasswordHash == Hashing.HashPassword(password))
                return true;
            else return false;
        }
*/
    }
}