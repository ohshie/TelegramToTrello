using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddTagToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly TaskNameRequest _taskNameRequest;

    public AddTagToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, 
        TaskNameRequest taskNameRequest, 
        CreatingTaskDbOperations creatingTaskDbOperations,
        Verifier verifier) : base(botClient, userDbOperations,
        taskDbOperations, verifier)
    {
        _taskNameRequest = taskNameRequest;
        _creatingTaskDbOperations = creatingTaskDbOperations;
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
        
        await _creatingTaskDbOperations.AddTag(task,tag);
        
        NextTask = _taskNameRequest;
        if (IsTemplate) NextTask.IsTemplate = true;
    }
}