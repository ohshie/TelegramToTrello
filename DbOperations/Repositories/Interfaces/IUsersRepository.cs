using Telegram.Bot.Types;

namespace TelegramToTrello;

public interface IUsersRepository : IRepository<User>
{
   Task<User> GetUserWithBoards(int id);

   Task<bool> CheckExist(int id);
}