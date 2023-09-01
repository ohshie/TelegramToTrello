using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class AssembleTaskFromTemplate : TaskCreationBaseHandler
{
    public AssembleTaskFromTemplate(ITelegramBotClient botClient, UserDbOperations dbOperations,
        TaskDbOperations taskDbOperations, Verifier verifier, 
        CreateKeyboardWithTags createKeyboardWithTags) : base(botClient, dbOperations, taskDbOperations,
        verifier)
    {
        NextTask = createKeyboardWithTags;
        NextTask.IsTemplate = true;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        int templateId = int.Parse(CallbackQuery.Data
            .Substring(CallbackList.Template.Length).Trim());

        await TaskDbOperations.FillTaskFromTemplate(task, templateId);
    }
}