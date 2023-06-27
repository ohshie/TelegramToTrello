using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddTableToTask : TaskCreationBaseHandler
{
    private bool IsEdit { get; set; }
    
    public AddTableToTask(CallbackQuery callback, ITelegramBotClient botClient, bool isEdit = false) : base(callback, botClient)
    {
        NextTask = new CreateKeyboardWithTags(callback,botClient);
        IsEdit = isEdit;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string listName = string.Empty;
        if (IsEdit)
        {
            listName = CallbackQuery.Data.Substring("/editlist".Length).Trim(); 
        }
        else
        {
            listName = CallbackQuery.Data.Substring("/list".Length).Trim();
        }
        

        CreatingTaskDbOperations dbOperations = new(user, task);
        bool listExist = await dbOperations.AddTable(listName);
        if (!listExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose list name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            NextTask = null;
        }

        if (IsEdit)
        {
            TaskDbOperations taskDbOperations = new();
            await taskDbOperations.ToggleEditModeForTask(task);
            NextTask = new CreateKeyboardWithUsers(CallbackQuery,BotClient, isEdit: true);
        }
    }
}