namespace TelegramToTrello;

public interface ITableRepository : IRepository<Table>
{
    Task<Table> GetByNameAndBoardId(string tableName, string trelloBoardId);

    Task AddRange(IEnumerable<Table> entity);

    Task RemoveRange(IEnumerable<Table> entity);
}