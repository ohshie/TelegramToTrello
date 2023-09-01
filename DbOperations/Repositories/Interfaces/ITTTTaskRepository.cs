namespace TelegramToTrello;

public interface ITTTTaskRepository : IRepository<TTTTask>
{
    public Task<bool> CheckIfExist(int userId);
}