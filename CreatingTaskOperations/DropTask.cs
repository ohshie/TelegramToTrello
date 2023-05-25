using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class DropTask : TaskCreationBaseHandler
{
    public DropTask(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient)
    {}
    
    public DropTask(Message message, ITelegramBotClient botClient) : base(message, botClient)
    {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        TaskDbOperations dbOperations = new();
        await dbOperations.RemoveEntry(task);

        if (CallbackQuery != null)
        {
            await BotClient.DeleteMessageAsync(chatId: Message.Chat.Id, Message.MessageId);
        }
        
        await BotClient.SendTextMessageAsync(chatId: Message.Chat.Id,
            text: "Task removed. You can start new one with /newtask");
    }
}