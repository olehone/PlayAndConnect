using PlayAndConnect.Models;
namespace PlayAndConnect.Data.Interfaces
{
    public interface IUserDb : IBaseDb<PlayAndConnect.Models.User>
    {
        Task<User?> GetUserByPassword(string login, string password);
        Task<User?> GetUserByLogin(string? login);
        Task<User?> Create(string login, string password);
        Task<User?> GetUserById(int? id);
        Task<ICollection<User>?> GetUsersListWithGame(Game? game);
        /*Task<PlayAndConnect.Models.User?> GetByName(string name);
        Task<bool> Verify (string name, string password);
        ICollection<PlayAndConnect.Models.Game> GetGames(PlayAndConnect.Models.User user);
        ICollection<PlayAndConnect.Models.Like> GetLikes(PlayAndConnect.Models.User user);
        ICollection<PlayAndConnect.Models.Chat> GetChats(PlayAndConnect.Models.User user);
        ICollection<PlayAndConnect.Models.Message> GetMessages(PlayAndConnect.Models.User user, PlayAndConnect.Models.Chat chat);
*/
    }
}
