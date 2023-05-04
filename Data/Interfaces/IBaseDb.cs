namespace PlayAndConnect.Data.Interfaces
{
    public interface IBaseDb<T>
    {
         Task<bool> Create (T entity);
         Task<T?> Get(int id);
         Task<bool> Delete (int id);
         Task<bool> Edit (T entity, int id);
    }
}