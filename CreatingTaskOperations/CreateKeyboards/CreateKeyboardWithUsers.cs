using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.CreatingTaskOperations;

public class CreateKeyboardWithUsers : TaskCreationOperator
{
    public CreateKeyboardWithUsers(Message message, ITelegramBotClient botClient) : base(message, botClient) {}

    public CreateKeyboardWithUsers(CallbackQuery callbackQuery, ITelegramBotClient botClient) : base(callbackQuery, botClient) {}
    
    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        InlineKeyboardMarkup replyKeyboardMarkup = await KeyboardParticipants(task);

        if (CallbackQuery == null)
        {
            await BotClient.SendTextMessageAsync(text: "choose participant from a list",
                chatId: Message.Chat.Id,
                replyMarkup: replyKeyboardMarkup);
            return;
        }
        
        await Task.WhenAll(
            BotClient.EditMessageTextAsync(chatId: CallbackQuery.Message.Chat.Id,
                messageId: CallbackQuery.Message.MessageId, 
                text: $"choose another participant from a list"),
            
            BotClient.EditMessageReplyMarkupAsync(chatId: Message.Chat.Id, 
                messageId:CallbackQuery.Message.MessageId,
                replyMarkup: replyKeyboardMarkup));
    }
    
    private async Task<InlineKeyboardMarkup> KeyboardParticipants(TTTTask task)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        DbOperations dbOperations = new DbOperations();
        Board taskBoard = await dbOperations.RetrieveBoard(task.Id, task.TrelloBoardId);

        if (taskBoard != null)
        {
            IEnumerable<UsersOnBoard> filteredUsers = taskBoard.UsersOnBoards;
            
            if (task.TaskPartName.Length > 0)
            {
                string addedUsers = UserTask.TaskPartName.Remove(task.TaskPartName.Length-1);
                List<string> addedUsersList = addedUsers.Split(',').ToList();
                filteredUsers = taskBoard.UsersOnBoards.Where(uob => !addedUsersList.Contains(uob.Name));
            }

            foreach (var user in filteredUsers)
            {
                keyboardButtonsList.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"{user.Name}",$"/name {user.Name}")});
            }
            
            keyboardButtonsList.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("press this when done","/name press this when done") });
        }

        InlineKeyboardMarkup replyKeyboardMarkup = new(keyboardButtonsList);

        return replyKeyboardMarkup;
    }
}