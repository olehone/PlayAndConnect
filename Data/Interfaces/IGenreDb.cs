namespace PlayAndConnect.Data.Interfaces
{
    public interface IGenreDb: IBaseDb<PlayAndConnect.Models.Genre>
    {
         ICollection<PlayAndConnect.Models.Game> GetGamesByGenre (PlayAndConnect.Models.Genre genre);
    }
}