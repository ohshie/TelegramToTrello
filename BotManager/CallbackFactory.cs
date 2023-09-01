using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;
using TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;
using TelegramToTrello.TaskManager.CurrentTaskOperations;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.AddToTemplate;

namespace TelegramToTrello.TaskManager;

public class CallbackFactory
{
    public CallbackFactory(CreateKeyboardWithBoards createKeyboardWithBoards,
        AddBoardToTask addBoardToTask,
        AddTableToTask addTableToTask,
        AddTagToTask addTagToTask,
        AddDateToTask addDateToTask,
        AddParticipantToTask addParticipantToTask,
        PushTask pushTask,
        TaskDateRequest taskDateRequest,
        TaskNameRequest taskNameRequest,
        TaskDescriptionRequest taskDescriptionRequest,
        DropTask dropTask,
        TaskInfoDisplay taskInfoDisplay,
        MarkTaskAsCompleted markTaskAsCompleted,
        AddAttachmentToTask addAttachmentToTask,
        AttachmentRequest attachmentRequest,
        AddBoardToTemplate addBoardToTemplate,
        AddTableToTemplate addTableToTemplate,
        ConfirmTemplate confirmTemplate,
        AssembleTaskFromTemplate assembleTaskFromTemplate,
        CreateKeyboardWithTags createKeyboardWithTags
    )
    {
        _botTaskFactory = new()
        {
            { CallbackList.Board, (callbackQuery) =>  addBoardToTask.Execute(callbackQuery) },
            { CallbackList.List, (callbackQuery) =>  addTableToTask.Execute(callbackQuery) },
            { CallbackList.Tag, (callbackQuery) =>  addTagToTask.Execute(callbackQuery) },
            { CallbackList.Name, (callbackQuery) =>  addParticipantToTask.Execute(callbackQuery) },
            { CallbackList.Push, (callbackQuery) =>  pushTask.Execute(callbackQuery) },
            { CallbackList.EditTaskBoardAndTable, (callbackQuery) =>  createKeyboardWithBoards.Execute(callbackQuery, isEdit: true) },
            { CallbackList.TaskEditboard, (callbackQuery) => addBoardToTask.Execute(callbackQuery, isEdit: true) },
            { CallbackList.TaskEditlist, (callbackQuery) => addTableToTask.Execute(callbackQuery, isEdit: true) },
            { CallbackList.TaskEditdate, (callbackQuery) => taskDateRequest.Execute(callbackQuery, isEdit: true) },
            { CallbackList.TaskEditname, (callbackQuery) =>  taskNameRequest.Execute(callbackQuery,isEdit: true) },
            { CallbackList.TaskEditdesc, (callbackQuery) =>  taskDescriptionRequest.Execute(callbackQuery, isEdit: true) },
            { CallbackList.Drop, (callbackQuery) =>  dropTask.Execute(callbackQuery) },
            { CallbackList.Autodate, (callbackQuery) =>  addDateToTask.Execute(callbackQuery) },
            { CallbackList.Edittask, taskInfoDisplay.Execute },
            { CallbackList.Taskcomplete, markTaskAsCompleted.Execute },
            { CallbackList.TaskMove, taskInfoDisplay.Execute },
            { CallbackList.AttachmentsDone, callbackQuery => addAttachmentToTask.Execute(callbackQuery)},
            { CallbackList.AddAttachment, callbackQuery => attachmentRequest.Execute(callbackQuery)},
            { CallbackList.TemplateBoard, addBoardToTemplate.Execute },
            { CallbackList.TemplateList, addTableToTemplate.Execute },
            { CallbackList.TemplateSave, confirmTemplate.Execute },
            { CallbackList.Template, query => assembleTaskFromTemplate.Execute(query) },
            { CallbackList.TemplateTag, query => createKeyboardWithTags.Execute(query, isTemplate: true) },
            { CallbackList.Skip, query => createKeyboardWithTags.Execute(query) }
        };
    }

    private readonly Dictionary<string, Func<CallbackQuery, Task>> _botTaskFactory;

    public async Task CallBackDataManager(CallbackQuery callbackQuery)
    {
        string key = callbackQuery.Data.Split(" ")[0];
        
        if (callbackQuery.Data != null && _botTaskFactory.ContainsKey(key))
        {
            await _botTaskFactory[key](callbackQuery);
        }
    }
}