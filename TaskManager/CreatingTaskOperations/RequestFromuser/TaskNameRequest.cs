using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

public class TaskNameRequest : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly MessageRemover _messageRemover;

    public TaskNameRequest(ITelegramBotClient botClient, 
        UserDbOperations dbOperations, 
        TaskDbOperations taskDbOperations,
        CreatingTaskDbOperations creatingTaskDbOperations, Verifier verifier, MessageRemover messageRemover) : base(botClient, dbOperations, taskDbOperations, verifier)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _messageRemover = messageRemover;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        if (IsTemplate)
        {
            await _creatingTaskDbOperations.AddPlaceholderName(task, isTemplate: true);
        }
        else
        {
            await _creatingTaskDbOperations.AddPlaceholderName(task);
        }
        
        
        await SendRequestToUser(task);
    }

    private async Task SendRequestToUser(TTTTask task)
    {
        if (IsEdit)
        {
            await TaskDbOperations.ToggleEditModeForTask(task);
        }
        
        await _messageRemover.Remove(CallbackQuery.Message.Chat.Id, CallbackQuery.Message.MessageId);

        if (IsTemplate)
        {
            await BotClient.SendTextMessageAsync(text: $"Add something to {task.TaskName
                .Substring(0, task.TaskName.Length - "##template##".Length).Trim()}",
                chatId: Message.Chat.Id);
            return;
        }
        
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: Message.Chat.Id);
    }
}