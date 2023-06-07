using PlayAndConnect.Models;
using PlayAndConnect.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace PlayAndConnect.Data.Interfaces
{
    public class GameDb : IGameDb
    {
        private readonly ApplicationDbContext _db;
        public GameDb(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Game?> GetGameByUser(User? user)
        {
            Console.WriteLine("try get user");
            if (user != null)
            {
                Console.WriteLine("heh");
                User? getUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                Console.WriteLine("try get user heh");
                if (getUser != null)
                    return await _db.Games.FirstOrDefaultAsync<Game>(g => g.Users != null && g.Users.Contains(user));
                else return null;
            }
            else
            {
                Console.WriteLine("hehoho");
                return null;
            }
        }
        public async Task<Game?> GetById(int? id)
        {
            if (id != null)
                return await _db.Games.FirstOrDefaultAsync<Game>(g => g.Id == id);
            else
                return null;
        }
        public async Task<List<Game>?> GetGamesByTitle(string? title)
        {
            if (title != null)
                return await _db.Games.Where(game => game.Title.ToLower().Contains(title.ToLower())).ToListAsync();
            else
                return null;
        }
        public async Task<List<Game>?> GetGamesByUser(User? user)
        {
            if (user != null)
                return await _db.Games.Where<Game>(g => g.Users != null && g.Users.Any(u => u == user)).ToListAsync();
            else
                return null;
        }
        public async Task<List<Game>?> GetGamesByGenre(Genre? genre)
        {
            if (genre != null)
                return await _db.Games.Where(game => game.Genre == genre).ToListAsync();
            else
                return null;
        }
        public async Task<List<Game>?> GetGamesWithGenresAndUsersByUser(User? user)
        {
            if (user != null)
                return await _db.Games
                    .Include(c => c.Genre)
                    .Where(g => g.Users != null && g.Users.Any(u => u.Id == user.Id))
                    .ToListAsync();
            else
                return null;
        }

        /*        public async Task<Info?> GetInfoByLogin(string? login)
                {
                    if (login != null)
                        return await _db.Infos.FirstOrDefaultAsync<Info>(i => i.User.Login == login);
                    else
                        return null;
                }*/
    }
}