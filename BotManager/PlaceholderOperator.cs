using Telegram.Bot.Types;
using TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.AddToTemplate;

namespace TelegramToTrello.BotManager;

public class PlaceholderOperator
{
    private readonly TaskDbOperations _taskDbOperations;
    private readonly AddNameToTask _addNameToTask;
    private readonly AddDescriptionToTask _addDescriptionToTask;
    private readonly AddDateToTask _addDateToTask;
    private readonly AddAttachmentToTask _addAttachmentToTask;
    private readonly TemplatesDbOperations _templatesDbOperations;
    private readonly AddNameToTemplate _addNameToTemplate;
    private readonly AddDescToTemplate _addDescToTemplate;
    private readonly UserDbOperations _userDbOperations;

    public PlaceholderOperator(UserDbOperations userDbOperations, 
        TaskDbOperations taskDbOperations,
        AddNameToTask addNameToTask,
        AddDescriptionToTask addDescriptionToTask,
        AddDateToTask addDateToTask,
        AddAttachmentToTask addAttachmentToTask,
        TemplatesDbOperations templatesDbOperations,
        AddNameToTemplate addNameToTemplate,
        AddDescToTemplate addDescToTemplate)
    {
        _userDbOperations = userDbOperations;
        _taskDbOperations = taskDbOperations;
        _addNameToTask = addNameToTask;
        _addDescriptionToTask = addDescriptionToTask;
        _addDateToTask = addDateToTask;
        _addAttachmentToTask = addAttachmentToTask;
        _templatesDbOperations = templatesDbOperations;
        _addNameToTemplate = addNameToTemplate;
        _addDescToTemplate = addDescToTemplate;
    }

    public async Task SortMessage(Message message)
    {
        User? trelloUser = await _userDbOperations.RetrieveTrelloUser((int)message.Chat.Id);
        if (trelloUser is null) return;
        
        TTTTask? task = await _taskDbOperations.RetrieveUserTask(trelloUser.TelegramId);
        if (task is not null)
        {
            if (await TaskSort(message, task)) return;
        }

        Template template = await _templatesDbOperations.GetIncompleteTemplate(trelloUser.TelegramId);
        if (template is not null)
        {
            if (await TemplateSort(message, template)) return;
        }
    }

    private async Task<bool> TemplateSort(Message message, Template template)
    {
        if (template.TaskName == "%%tempName%%#")
        {
            await _addNameToTemplate.Execute(message);
            return true;
        }

        if (template.TaskDesc == "%%tempDesc%%#")
        {
            await _addDescToTemplate.Execute(message);
            return true;
        }

        return false;
    }

    private async Task<bool> TaskSort(Message message, TTTTask task)
    {
        if (task.TaskName == "###tempname###")
        {
            await _addNameToTask.Execute(message);
            return true;
        }

        if (task.TaskDesc is not null && task.TaskName.Contains("##template##"))
        {
            await _addNameToTask.Execute(message, isTemplate: true);
            return true;
        }

        if (task.TaskDesc == "###tempdesc###")
        {
            await _addDescriptionToTask.Execute(message);
            return true;
        }

        if (task.TaskDesc is not null && task.TaskDesc.Contains("##template##"))
        {
            await _addDescriptionToTask.Execute(message, isTemplate: true);
            return true;
        }

        if (task.Date == "###tempdate###")
        {
            await _addDateToTask.Execute(message);
            return true;
        }

        if (task.WaitingForAttachment)
        {
            if (message.Photo is null && message.Document is null && message.Video is null) return false;
            if (message.Document is not null && message.Document.FileSize > 25000000) return false;

            await _addAttachmentToTask.Execute(message);
            return true;
        }

        return false;
    }
}