namespace TelegramToTrello;

public interface ITrelloUsersRepository
{
    Task<UsersOnBoard> GetByNameAndBoardId(string name, string boardId);
}