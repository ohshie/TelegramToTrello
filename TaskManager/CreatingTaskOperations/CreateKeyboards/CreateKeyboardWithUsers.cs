using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithUsers : TaskCreationBaseHandler
{
    public CreateKeyboardWithUsers(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        UsersKeyboard usersKeyboard, 
        Verifier verifier, 
        BotMessenger botMessenger, TaskDbOperations taskDbOperations) 
        : base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _usersKeyboard = usersKeyboard;
    }

    private readonly UsersKeyboard _usersKeyboard;

    protected override async Task HandleTask(User user, TTTTask task)
    {
        if (IsEdit)
        {
            await TaskDbOperations.ResetParticipants(task);
        }
        
        InlineKeyboardMarkup replyKeyboardMarkup = await _usersKeyboard.KeyboardParticipants(task);

        if (CallbackQuery == null)
        {
            await BotMessenger.SendMessage(text: "choose participant from a list",
                chatId: user.TelegramId, 
                replyKeyboardMarkup: replyKeyboardMarkup);
            return;
        }

        await BotMessenger.UpdateMessage(chatId: user.TelegramId,
            messageId: CallbackQuery.Message.MessageId,
            text: $"Choose participant from a list", 
            replyKeyboardMarkup);
    }
}