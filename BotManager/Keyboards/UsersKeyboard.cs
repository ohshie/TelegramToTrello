using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class UsersKeyboard
{
    private readonly DbOperations _dbOperations;

    public UsersKeyboard(DbOperations dbOperations)
    {
        _dbOperations = dbOperations;
    }

    public async Task<InlineKeyboardMarkup?> KeyboardParticipants(TTTTask task)
    {
        Board taskBoard = await _dbOperations.RetrieveBoard(task.Id, task.TrelloBoardId!);

        if (taskBoard != null!)
        {
            var filteredUsers = FilterUsers(taskBoard, task);
            
            if (filteredUsers.Count > 8)
            {
                InlineKeyboardMarkup replyKeyboardMarkup = new(TwoRowKeyboard(filteredUsers));
                return replyKeyboardMarkup;
            }
            else
            {
                InlineKeyboardMarkup replyKeyboardMarkup = new(SingleRowKeyboard(filteredUsers));
                return replyKeyboardMarkup;
            }
        }
        return null;
    }

    private List<InlineKeyboardButton[]> TwoRowKeyboard(List<UsersOnBoard> filteredUsers)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();
        for (int i = 0; i < filteredUsers.Count; i +=2)
        {
            if (i < filteredUsers.Count-1)
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{filteredUsers[i].Name}",
                        $"/name {filteredUsers[i].Name}"),
                    InlineKeyboardButton.WithCallbackData($"{filteredUsers[i+1].Name}",
                        $"/name {filteredUsers[i+1].Name}")
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{filteredUsers[i].Name}",
                        $"/name {filteredUsers[i].Name}")
                });
            }
        }
        keyboardButtonsList.Add(new[] { InlineKeyboardButton.WithCallbackData("press this when done","/name press this when done") });

        return keyboardButtonsList;
    }

    private List<InlineKeyboardButton[]> SingleRowKeyboard(List<UsersOnBoard> filteredUsers)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        foreach (var user in filteredUsers)
        {
            keyboardButtonsList.Add(new[] { InlineKeyboardButton.WithCallbackData($"{user.Name}",$"/name {user.Name}") });
        }
        
        keyboardButtonsList.Add(new[] { InlineKeyboardButton.WithCallbackData("press this when done","/name press this when done") });
        return keyboardButtonsList;
    }

    private List<UsersOnBoard> FilterUsers(Board taskBoard, TTTTask task)
    {
        var filteredUsers = new List<UsersOnBoard>(taskBoard.UsersOnBoards!);

        if (task.TaskPartName != null && task.TaskPartName.Length > 0)
        {
            string? addedUsers = task?.TaskPartName?.Remove(task.TaskPartName.Length-1);
            List<string> addedUsersList = addedUsers!.Split(',').ToList();
            filteredUsers = new List<UsersOnBoard>(taskBoard.UsersOnBoards!.Where(uob => !addedUsersList.Contains(uob.Name!)));
        }

        return filteredUsers;
    }
}