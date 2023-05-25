using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotActions;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.UserRegistration;

namespace TelegramToTrello;

public class ActionsFactory
{
    private Dictionary<string, Func<Message, ITelegramBotClient, Task>> BotTaskFactory =
        new()
        {
            { "/start", (message, botClient) => new UserRegistrationHandler(message, botClient).Authenticate() },
            { "/register", (message, botClient) => new UserRegistrationHandler(message, botClient).Authenticate() },
            { "/SyncBoards", (message, botClient) => new UserRegistrationHandler(message, botClient).SyncBoards() },
            { "/newtask", (message, botClient) => new StartTaskCreation(message, botClient).CreateTask() },
            { "/notifications", (message, botClient) => new BotNotificationCentre(message,botClient).ToggleNotificationsForUser()},
            { "/drop", (message, botClient) => new DropTask(message,botClient).Execute()}
        };

    public async Task BotActionFactory(Message message, ITelegramBotClient botClient)
    {
        if (BotTaskFactory.ContainsKey(message.Text))
        {
            await BotTaskFactory[message.Text](message, botClient);
        }
    }
}