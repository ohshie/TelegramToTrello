using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.BotManager;

public class CurrentTasksKeyboard
{
    public InlineKeyboardMarkup CreateKeyboard(Dictionary<string, TrelloOperations.TrelloCard> cards)
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
                InlineKeyboardButton.WithCallbackData($"Task 1", $"{CallbackList.Edittask} {cards[card.Key].Id}")
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
                    InlineKeyboardButton.WithCallbackData($"Task {++counter}", $"{CallbackList.Edittask} {cardList[i].Id}"),
                    InlineKeyboardButton.WithCallbackData($"Task {++counter}", $"{CallbackList.Edittask} {cardList[i + 1].Id}")
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"Task {++counter}", $"{CallbackList.Edittask} {cardList[i].Id}")
                });
            }
        }
    }
}