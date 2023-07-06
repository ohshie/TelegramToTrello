namespace TelegramToTrello;

public interface IBoardRepository : IRepository<Board>
{
    Task DeleteRange(IEnumerable<Board> entity);
}