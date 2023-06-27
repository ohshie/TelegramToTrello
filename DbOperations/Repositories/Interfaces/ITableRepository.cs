namespace TelegramToTrello;

public interface ITableRepository : IRepository<Table>
{
    Task<Table> GetByNameAndBoardId(string tableName, string trelloBoardId);
}