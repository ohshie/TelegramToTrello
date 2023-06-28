using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.CreatingTaskOperations;

public class PushTask : TaskCreationBaseHandler
{
    private readonly TrelloOperations _trelloOperations;

    public PushTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, TrelloOperations trelloOperations) : base(botClient, userDbOperations, taskDbOperations)
    {
        _trelloOperations = trelloOperations;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        bool success = await _trelloOperations.PushTaskToTrello(task);
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
        await TaskDbOperations.RemoveEntry(task);
    }
}