namespace TelegramToTrello;

public interface ITrelloUsersRepository : IRepository<UsersOnBoard>
{
    Task<UsersOnBoard> GetByNameAndBoardId(string name, string boardId);

    Task AddRange(IEnumerable<UsersOnBoard> entity);

    Task RemoveRange(IEnumerable<UsersOnBoard> entity);

    Task<UsersOnBoard> GetByTrelloIdAndBoardId(string id, string boardId);
}