using PlayAndConnect.Models;
namespace PlayAndConnect.Data.Interfaces
{
    public interface IUserDb : IBaseDb<PlayAndConnect.Models.User>
    {
        Task<User?> GetUserByPassword(string login, string password);
        Task<User?> GetUserByLogin(string? login);
        Task<User?> Create(string login, string password);
        Task<ICollection<User>?> GetUsersListWithGame(Game? game);
    }
}
