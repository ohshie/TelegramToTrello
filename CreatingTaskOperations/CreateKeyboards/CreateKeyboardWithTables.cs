using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.CreatingTaskOperations;

public class CreateKeyboardWithTables : TaskCreationOperator
{
    public CreateKeyboardWithTables(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient) {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        InlineKeyboardMarkup replyKeyboardMarkup = await KeyboardTableChoice(user, task);
        await Task.WhenAll(
            BotClient.EditMessageTextAsync(chatId: CallbackQuery.Message.Chat.Id,
                messageId: CallbackQuery.Message.MessageId, 
                text: $"Now choose list on {task.TrelloBoardName}"),
            
        BotClient.EditMessageReplyMarkupAsync(chatId: Message.Chat.Id, 
            messageId:CallbackQuery.Message.MessageId,
            replyMarkup: replyKeyboardMarkup));
    }
    
    private async Task<InlineKeyboardMarkup> KeyboardTableChoice(RegisteredUser user, TTTTask task)
    {
        DbOperations dbOperations = new DbOperations();
        Board selectedBoard = await dbOperations.RetrieveBoard(user.TelegramId, task.TrelloBoardId);

        List<InlineKeyboardButton[]> keyboardButtonsList = new List<InlineKeyboardButton[]>();

        foreach (var table in selectedBoard.Tables)
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData($"{table.Name}",$"/list {table.Name}")
            });
        }

        InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(keyboardButtonsList);

        return replyKeyboardMarkup;
    }
}