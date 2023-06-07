using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayAndConnect.Models;
namespace PlayAndConnect.Data.Interfaces
{
    public interface IGameDb : IBaseDb<Game>
    {
        Task<Game?> GetGameByUser(User? user);
        Task<List<Game>?> GetGamesByTitle(string? title);
        Task<List<Game>?> GetGamesByGenre(Genre? genre);
        Task<List<Game>?> GetGamesByUser(User? user);
        Task<List<Game>?> GetGamesWithGenresAndUsersByUser(User? user);
    }
}