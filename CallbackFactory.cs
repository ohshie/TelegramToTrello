using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CurrentTaskOperations;

namespace TelegramToTrello.CreatingTaskOperations;

public class CallbackFactory
{
    private readonly Dictionary<string, Func<CallbackQuery, ITelegramBotClient, Task>> BotTaskFactory = 
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
            { "/edittask", (callbackQuery, botClient) => new TaskInfoDisplay(callbackQuery, botClient).Execute() },
            { "/taskComplete", (callbackQuery, botClient) => new MarkTaskAsCompleted(callbackQuery, botClient).Execute() },
            { "/taskMove", (callbackQuery, botClient) => new TaskInfoDisplay(callbackQuery, botClient).Execute() }
        };
    
    
    public async Task CallBackDataManager(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        string key = callbackQuery.Data.Split(" ")[0];
        
        if (callbackQuery.Data != null && BotTaskFactory.ContainsKey(key))
        {
            await BotTaskFactory[key](callbackQuery,botClient);
        }
    }
}