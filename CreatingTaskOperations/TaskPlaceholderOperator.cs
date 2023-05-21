using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.BotActions;

public class TaskPlaceholderOperator
{
    public async Task SortMessage(Message message, ITelegramBotClient botClient)
    {
        TTTTask? task = await GetTrelloUserAndTask(message);
        if (task == null) return;
        
        if (task.TaskName == "###tempname###")
        {
            AddNameToTask addNameToTask = new(message, botClient);
            await addNameToTask.Execute();
        }
        
        if (task.TaskDesc == "###tempdesc###")
        {
            AddDescriptionToTask addDescriptionToTask = new(message, botClient);
            await addDescriptionToTask.Execute();
        }
        
        if (task.Date == "###tempdate###")
        {
            AddDateToTask addDateToTask = new(message, botClient);
            await addDateToTask.Execute();
        }
    }
    
    private async Task<TTTTask?> GetTrelloUserAndTask(Message message)
    {
        DbOperations dbOperations = new DbOperations();
        
        RegisteredUser trelloUser = await dbOperations.RetrieveTrelloUser((int)message.Chat.Id);
        if (trelloUser != null)
        {   
            var task = await dbOperations.RetrieveUserTask((int)message.Chat.Id);
            return task;
        }

        return null;
    }
}