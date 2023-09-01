using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithTags : TaskCreationBaseHandler
{
    public CreateKeyboardWithTags(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, TagsKeyboard tagsKeyboard, Verifier verifier) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _tagsKeyboard = tagsKeyboard;
    }

    private readonly TagsKeyboard _tagsKeyboard;
    
    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        InlineKeyboardMarkup replyKeyboardMarkup = _tagsKeyboard.KeyboardTagChoice();

        if (IsTemplate)
        {
            await BotClient.SendTextMessageAsync(chatId: user.TelegramId,
                text: $"Choose channel tag according to your task channel", replyMarkup: replyKeyboardMarkup);
        }
        else
        {
            await BotClient.EditMessageTextAsync(chatId: user.TelegramId,
                messageId: CallbackQuery.Message.MessageId,
                text: $"Choose channel tag according to your task channel", replyMarkup: replyKeyboardMarkup);
        }
    }
}