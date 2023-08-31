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
        UserDbOperations userDbOperations, MessageRemover messageRemover)
    {
        BotClient = botClient;
        _trelloOperations = trelloOperations;
        _userDbOperations = userDbOperations;
        _messageRemover = messageRemover;
    }
    
    private ITelegramBotClient BotClient { get; }
    private readonly UserDbOperations _userDbOperations;
    private readonly TrelloOperations _trelloOperations;
    private readonly MessageRemover _messageRemover;

    public async Task Execute(Message message)
    {
        RegisteredUser user = await _userDbOperations.RetrieveTrelloUser((int)message.From.Id);
        if (user != null)
        {
            var cards = await _trelloOperations.GetSubscribedTasks(user);
            
            await _messageRemover.Remove(message.Chat.Id, message.MessageId);
            
            InlineKeyboardMarkup keyboardMarkup = CreateKeyboard(cards);

            var botMessageText = CreateBotMessageText(cards);

            await BotClient.SendTextMessageAsync(text: botMessageText,
                chatId: message.Chat.Id, 
                replyMarkup: keyboardMarkup);
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

    private InlineKeyboardMarkup CreateKeyboard(Dictionary<string, TrelloOperations.TrelloCard> cards)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        var cardList = cards.Values.ToList();
        
        if (cards.Count > 3)
        {
            TwoRowKeyboard(cardList, keyboardButtonsList);
        }
        else
        {
            SingleRowKeyboard(cards, keyboardButtonsList);
        }
        
        InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup(keyboardButtonsList);
        
        return keyboardMarkup;
    }

    private void SingleRowKeyboard(Dictionary<string, TrelloOperations.TrelloCard> cards, List<InlineKeyboardButton[]> keyboardButtonsList)
    {
        int counter = 0;
        foreach (var card in cards)
        {
            keyboardButtonsList.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData($"Task 1", $"/edittask {cards[card.Key].Id}")
            });
        }
    }

    private void TwoRowKeyboard(List<TrelloOperations.TrelloCard> cardList, List<InlineKeyboardButton[]> keyboardButtonsList)
    {
        int counter = 0;
        for (int i = 0; i < cardList.Count; i += 2)
        {
            if (i < cardList.Count - 1)
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"Task {++counter}", $"/edittask {cardList[i].Id}"),
                    InlineKeyboardButton.WithCallbackData($"Task {++counter}", $"/edittask {cardList[i + 1].Id}")
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"Task {++counter}", $"/edittask {cardList[i].Id}")
                });
            }
        }
    }
}