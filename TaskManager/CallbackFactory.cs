using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;
using TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;
using TelegramToTrello.TaskManager.CurrentTaskOperations;

namespace TelegramToTrello.TaskManager;

public class CallbackFactory
{
    private readonly ITelegramBotClient _botClient;
    private readonly CreateKeyboardWithBoards _createKeyboardWithBoards;
    private readonly AddBoardToTask _addBoardToTask;
    private readonly AddTableToTask _addTableToTask;
    private readonly AddTagToTask _addTagToTask;
    private readonly AddDateToTask _addDateToTask;
    private readonly AddParticipantToTask _addParticipantToTask;
    private readonly PushTask _pushTask;
    private readonly TaskDateRequest _taskDateRequest;
    private readonly TaskNameRequest _taskNameRequest;
    private readonly TaskDescriptionRequest _taskDescriptionRequest;
    private readonly DropTask _dropTask;
    private readonly TaskInfoDisplay _taskInfoDisplay;
    private readonly MarkTaskAsCompleted _markTaskAsCompleted;

    public CallbackFactory(ITelegramBotClient botClient, 
        CreateKeyboardWithBoards createKeyboardWithBoards,
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
        MarkTaskAsCompleted markTaskAsCompleted
    )
    {
        _botClient = botClient;
        _createKeyboardWithBoards = createKeyboardWithBoards;
        _addBoardToTask = addBoardToTask;
        _addTableToTask = addTableToTask;
        _addTagToTask = addTagToTask;
        _addDateToTask = addDateToTask;
        _addParticipantToTask = addParticipantToTask;
        _pushTask = pushTask;
        _taskDateRequest = taskDateRequest;
        _taskNameRequest = taskNameRequest;
        _taskDescriptionRequest = taskDescriptionRequest;
        _dropTask = dropTask;
        _taskInfoDisplay = taskInfoDisplay;
        _markTaskAsCompleted = markTaskAsCompleted;

        _botTaskFactory = new()
        {
            { "/board", (callbackQuery) =>  _addBoardToTask.Execute(callbackQuery) },
            { "/list", (callbackQuery) =>  _addTableToTask.Execute(callbackQuery) },
            { "/tag", (callbackQuery) =>  _addTagToTask.Execute(callbackQuery) },
            { "/name", (callbackQuery) =>  _addParticipantToTask.Execute(callbackQuery) },
            { "/push", (callbackQuery) =>  _pushTask.Execute(callbackQuery) },
            { "/edittaskboardandtable", (callbackQuery) =>  _createKeyboardWithBoards.Execute(callbackQuery, isEdit: true) },
            { "/editboard", (callbackQuery) => _addBoardToTask.Execute(callbackQuery, isEdit: true) },
            { "/editlist", (callbackQuery) => _addTableToTask.Execute(callbackQuery, isEdit: true) },
            { "/editdate", (callbackQuery) => _taskDateRequest.Execute(callbackQuery, isEdit: true) },
            { "/editname", (callbackQuery) =>  _taskNameRequest.Execute(callbackQuery,isEdit: true) },
            { "/editdesc", (callbackQuery) =>  _taskDescriptionRequest.Execute(callbackQuery, isEdit: true) },
            { "/drop", (callbackQuery) =>  _dropTask.Execute(callbackQuery) },
            { "/autodate", (callbackQuery) =>  _addDateToTask.Execute(callbackQuery) },
            { "/edittask", (callbackQuery) => _taskInfoDisplay.Execute(callbackQuery) },
            { "/taskComplete", (callbackQuery) =>  _markTaskAsCompleted.Execute(callbackQuery) },
            { "/taskMove", (callbackQuery) => _taskInfoDisplay.Execute(callbackQuery) }
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