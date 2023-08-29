using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithUsers : TaskCreationBaseHandler
{
    private readonly DbOperations _dbOperations;

    public CreateKeyboardWithUsers(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, DbOperations dbOperations) : base(botClient, userDbOperations, taskDbOperations)
    {
        _dbOperations = dbOperations;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        if (IsEdit)
        {
            await TaskDbOperations.ResetParticipants(task);
        }
        
        InlineKeyboardMarkup? replyKeyboardMarkup = await KeyboardParticipants(task);

        if (CallbackQuery == null)
        {
            await BotClient.SendTextMessageAsync(text: "choose participant from a list",
                chatId: Message!.Chat.Id,
                replyMarkup: replyKeyboardMarkup);
            return;
        }

        await BotClient.EditMessageTextAsync(chatId: CallbackQuery.Message!.Chat.Id,
            messageId: CallbackQuery.Message.MessageId,
            text: $"Choose participant from a list", 
            replyMarkup: replyKeyboardMarkup);
    }
    
    private async Task<InlineKeyboardMarkup?> KeyboardParticipants(TTTTask task)
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