using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.TaskManager.CurrentTaskOperations;

public class CurrentTasksDisplay
{
    public CurrentTasksDisplay(ITelegramBotClient botClient, 
        TrelloOperations trelloOperations, 
        UserDbOperations userDbOperations, BotMessenger botMessenger,
        CurrentTasksKeyboard currentTasksKeyboard)
    {
        BotClient = botClient;
        _trelloOperations = trelloOperations;
        _userDbOperations = userDbOperations;
        _botMessenger = botMessenger;
        _currentTasksKeyboard = currentTasksKeyboard;
    }
    
    private ITelegramBotClient BotClient { get; }
    private readonly UserDbOperations _userDbOperations;
    private readonly BotMessenger _botMessenger;
    private readonly CurrentTasksKeyboard _currentTasksKeyboard;
    private readonly TrelloOperations _trelloOperations;

    public async Task Execute(Message message)
    {
        User user = await _userDbOperations.RetrieveTrelloUser((int)message.From.Id);
        if (user != null)
        {
            var cards = await _trelloOperations.GetSubscribedTasks(user);
            
            await _botMessenger.RemoveMessage(user.TelegramId, message.MessageId);
            
            InlineKeyboardMarkup keyboardMarkup = _currentTasksKeyboard.CreateKeyboard(cards);

            var botMessageText = CreateBotMessageText(cards);

            await _botMessenger.SendMessage(text: botMessageText,
                chatId: user.TelegramId, 
                replyKeyboardMarkup: keyboardMarkup);
        }
    }

    private string CreateBotMessageText(Dictionary<string, TrelloOperations.TrelloCard> cards)
    {
        string botMessageText = string.Empty;
        int counter = 0;
        foreach (var card in cards)
        {
            botMessageText += $"Task {++counter}\n" +
                              $"Name: {cards[card.Key].Name}\n" +
                              $"Due: {cards[card.Key].Due}\n" +
                              $"Url: {cards[card.Key].Url}\n";
        }

        botMessageText += "\n\n" +
                          "Choose task to modify";
        return botMessageText;
    }
}