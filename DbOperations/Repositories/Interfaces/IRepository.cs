namespace TelegramToTrello;

public interface IRepository<TEntity> where TEntity : class
{ 
    Task<TEntity> Get(int id);
    Task<TEntity> Get(string id);
    Task<IEnumerable<TEntity>> GetAll();
    Task Add(TEntity entity);
    Task Update(TEntity entity);
    Task Delete(TEntity entity);
}