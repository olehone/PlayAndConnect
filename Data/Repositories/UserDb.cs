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
        public async Task<User?> GetById(int? id)
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
    }
}