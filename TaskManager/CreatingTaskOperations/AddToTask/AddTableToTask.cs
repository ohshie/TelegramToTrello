using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddTableToTask : TaskCreationBaseHandler
{
    private readonly CreateKeyboardWithTags _createKeyboardWithTags;
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly CreateKeyboardWithUsers _createKeyboardWithUsers;

    public AddTableToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, CreateKeyboardWithTags createKeyboardWithTags,
        CreatingTaskDbOperations creatingTaskDbOperations, CreateKeyboardWithUsers createKeyboardWithUsers,
        Verifier verifier) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _createKeyboardWithTags = createKeyboardWithTags;
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _createKeyboardWithUsers = createKeyboardWithUsers;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string listName;
        
        if (IsEdit)
        {
            listName = CallbackQuery.Data.Substring("/editlist".Length).Trim(); 
        }
        else
        {
            listName = CallbackQuery.Data.Substring("/list".Length).Trim();
        }
        
        bool listExist = await _creatingTaskDbOperations.AddTable(task, listName);
        if (!listExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose list name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            NextTask = null;
        }

        if (IsEdit)
        {
            await TaskDbOperations.ToggleEditModeForTask(task);
            NextTask = _createKeyboardWithUsers;
            NextTask.IsEdit = true;
        }
        else
        {
            NextTask = _createKeyboardWithTags; 
        }
    }
}