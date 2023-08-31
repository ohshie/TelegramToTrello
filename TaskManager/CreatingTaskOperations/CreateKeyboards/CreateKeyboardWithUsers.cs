using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithUsers : TaskCreationBaseHandler
{
    public CreateKeyboardWithUsers(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, UsersKeyboard usersKeyboard, Verifier verifier) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _usersKeyboard = usersKeyboard;
    }

    private readonly UsersKeyboard _usersKeyboard;

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        if (IsEdit)
        {
            await TaskDbOperations.ResetParticipants(task);
        }
        
        InlineKeyboardMarkup? replyKeyboardMarkup = await _usersKeyboard.KeyboardParticipants(task);

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
}