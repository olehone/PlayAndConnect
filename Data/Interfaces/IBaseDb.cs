namespace PlayAndConnect.Data.Interfaces
{
    public interface IBaseDb<T>
    {
        Task<T?> GetById(int? id);
    }
}