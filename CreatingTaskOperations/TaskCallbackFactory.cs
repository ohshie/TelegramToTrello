using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskCallbackFactory
{
   
    
    private readonly Dictionary<string, Func<CallbackQuery, ITelegramBotClient, Task>> _taskFactoryByPrefix = 
        new(){
            { "/board", (callbackQuery, botClient) => new AddBoardToTask(callbackQuery, botClient).Execute() },
            { "/list", (callbackQuery, botClient) => new AddTableToTask(callbackQuery, botClient).Execute() },
            { "/tag", (callbackQuery, botClient) => new AddTagToTask(callbackQuery, botClient).Execute() },
            { "/name", (callbackQuery, botClient) => new AddParticipantToTask(callbackQuery, botClient).Execute() },
            { "/push", (callbackQuery, botClient) => new PushTask(callbackQuery, botClient).Execute() },
            { "/edittaskboardandtable", (callbackQuery, botClient) => new CreateKeyboardWithBoards(callbackQuery, botClient, isEdit: true).Execute() },
            { "/editboard", (callbackQuery, botClient) => new AddBoardToTask(callbackQuery, botClient, isEdit:true).Execute() },
            { "/editlist", (callbackQuery, botClient) => new AddTableToTask(callbackQuery, botClient, isEdit:true).Execute() },
            { "/editdate", (callbackQuery, botClient) => new TaskDateRequest(callbackQuery, botClient, isEdit:true).Execute() },
            { "/editname", (callbackQuery, botClient) => new TaskNameRequest(callbackQuery, botClient, isEdit:true).Execute() },
            { "/editdesc", (callbackQuery, botClient) => new TaskDescriptionRequest(callbackQuery, botClient, isEdit:true).Execute() },
            { "/drop", (callbackQuery, botClient) => new DropTask(callbackQuery, botClient).Execute() },
            { "/autodate", (callbackQuery, botClient) => new AddDateToTask(callbackQuery, botClient).Execute() },
        };
    
    
    public async Task CallBackDataManager(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        foreach (var key in _taskFactoryByPrefix.Keys)
        {
            if (callbackQuery.Data.StartsWith(key))
            {
                await _taskFactoryByPrefix[key](callbackQuery,botClient);
                return;
            }
        }
    }
}