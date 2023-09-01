using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithTags : TaskCreationBaseHandler
{
    public CreateKeyboardWithTags(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TagsKeyboard tagsKeyboard, Verifier verifier, BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _tagsKeyboard = tagsKeyboard;
    }

    private readonly TagsKeyboard _tagsKeyboard;
    
    protected override async Task HandleTask(User user, TTTTask task)
    {
        InlineKeyboardMarkup replyKeyboardMarkup = _tagsKeyboard.KeyboardTagChoice();

        if (IsTemplate)
        {
            await BotMessenger.SendMessage(chatId: user.TelegramId,
                text: $"Choose channel tag according to your task channel", 
                replyKeyboardMarkup: replyKeyboardMarkup);
        }
        else
        {
            await BotMessenger.UpdateMessage(chatId: user.TelegramId,
                messageId: CallbackQuery.Message.MessageId,
                text: $"Choose channel tag according to your task channel", 
                keyboardMarkup: replyKeyboardMarkup);
        }
    }
}