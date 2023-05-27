using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CurrentTaskOperations;

public class CurrentTasksDisplay
{
    public CurrentTasksDisplay(Message message, ITelegramBotClient botClient)
    {
        Message = message;
        BotClient = botClient;
    }

    private Message Message { get; }
    private ITelegramBotClient BotClient { get; }
    private readonly UserDbOperations _userDbOperations = new();
    private readonly TrelloOperations _trelloOperations = new();
    
    public async Task Execute()
    {
        RegisteredUser user = await _userDbOperations.RetrieveTrelloUser((int)Message.From.Id);
        if (user != null)
        {
            var cards = await _trelloOperations.GetSubscribedTasks(user);

            foreach (var card in cards)
            {
                Console.WriteLine($"{card.Value.Name}\n" +
                                  $"{card.Value.SubscribeStatus}");
            }
        }
    }
}