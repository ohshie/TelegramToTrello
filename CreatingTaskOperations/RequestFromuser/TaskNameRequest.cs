using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskNameRequest : TaskCreationOperator
{
    public TaskNameRequest(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient)
    {
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        CreatingTaskDbOperations dbOperations = new(user, task); 
        await dbOperations.AddPlaceholderName();
        
        await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id,messageId: CallbackQuery.Message.MessageId);
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: Message.Chat.Id);
    }
}