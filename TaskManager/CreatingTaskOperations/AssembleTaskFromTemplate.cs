using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class AssembleTaskFromTemplate : TaskCreationBaseHandler
{
    public AssembleTaskFromTemplate(ITelegramBotClient botClient, 
        UserDbOperations dbOperations, 
        Verifier verifier, 
        CreateKeyboardWithTags createKeyboardWithTags, BotMessenger botMessenger, TaskDbOperations taskDbOperations) 
        : base(botClient, dbOperations,
        verifier, botMessenger, taskDbOperations)
    {
        NextTask = createKeyboardWithTags;
        NextTask.IsTemplate = true;
    }

    protected override async Task HandleTask(TTTTask task)
    {
        int templateId = int.Parse(CallbackQuery.Data
            .Substring(CallbackList.Template.Length).Trim());

        await TaskDbOperations.FillTaskFromTemplate(task, templateId);
    }
}