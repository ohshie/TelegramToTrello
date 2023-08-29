using Telegram.Bot.Types;

namespace TelegramToTrello;

public interface IUsersRepository : IRepository<RegisteredUser>
{
   Task<RegisteredUser> GetUserWithBoards(int id);
}