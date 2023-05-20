using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class PushTask : TaskCreationOperator
{
    public PushTask(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient)
    {
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        TrelloOperations trelloOperations = new();
        
        bool success = await trelloOperations.PushTaskToTrello(task);
        if (success)
        {
            await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id,
                messageId: CallbackQuery.Message.MessageId);
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                text: "Task successfully created");
            await RemoveTaskFromDb(task);
        }
    }

    private async Task RemoveTaskFromDb(TTTTask task)
    {
        DbOperations dbOperations = new DbOperations();
        await dbOperations.RemoveEntry(task);
    }
}