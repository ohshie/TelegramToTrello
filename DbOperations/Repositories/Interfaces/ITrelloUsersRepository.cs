namespace TelegramToTrello;

public interface ITrelloUsersRepository : IRepository<UsersOnBoard>
{
    Task<UsersOnBoard> GetByNameAndBoardId(string name, string boardId);
}