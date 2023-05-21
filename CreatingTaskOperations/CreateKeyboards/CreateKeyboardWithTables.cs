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
        
        await BotClient.EditMessageTextAsync(chatId: CallbackQuery.Message.Chat.Id,
                messageId: CallbackQuery.Message.MessageId,
                text: $"Now choose list on {task.TrelloBoardName}");
            
        await BotClient.EditMessageReplyMarkupAsync(chatId: Message.Chat.Id, 
            messageId:CallbackQuery.Message.MessageId,
            replyMarkup: replyKeyboardMarkup);
    }
    
    private async Task<InlineKeyboardMarkup> KeyboardTableChoice(RegisteredUser user, TTTTask task)
    {
        DbOperations dbOperations = new DbOperations();
        Board selectedBoard = await dbOperations.RetrieveBoard(user.TelegramId, task.TrelloBoardId);

        if (selectedBoard.Tables.Count > 8)
        {
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(TwoRowKeyboard(selectedBoard));
            return replyKeyboardMarkup;
        }
        else
        {
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(SingleRowKeyboard(selectedBoard));
            return replyKeyboardMarkup;
        }
    }
    
    private List<InlineKeyboardButton[]> TwoRowKeyboard(Board board)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        var tables = board.Tables!.ToArray();
        for (int i = 0; i < tables.Length; i +=2)
        {
            if (i < tables.Length-1)
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tables[i]}",
                        $"/list {tables[i]}"),
                    InlineKeyboardButton.WithCallbackData($"{tables[i+1]}",
                        $"/list {tables[i+1]}")
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tables[i]}",
                        $"/list {tables[i]}")
                });
            }
        }
        return keyboardButtonsList;
    }

    private List<InlineKeyboardButton[]> SingleRowKeyboard(Board board)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        foreach (var table in board.Tables!)
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData($"{table.Name}",$"/list {table.Name}")
            }); 
        }
        
        return keyboardButtonsList;
    }
}