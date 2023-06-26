using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddTagToTask : TaskCreationBaseHandler
{
    public AddTagToTask(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient)
    {
        NextTask = new TaskNameRequest(callback, botClient);
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string tag = CallbackQuery.Data.Substring("/tag".Length).Trim();
        
        if (!(Enum.TryParse(typeof(ChanelTags), tag, true, out _)))
        {
            await BotClient.SendTextMessageAsync(text: "Please choose tag from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            NextTask = null;
            return;
        }

        CreatingTaskDbOperations dbOperations = new(user, task); 
        await dbOperations.AddTag(tag);
    }
}