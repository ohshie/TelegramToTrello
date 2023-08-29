using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class BotSettingsMenu
{
    private readonly ITelegramBotClient _botClient;
    private readonly UserDbOperations _userDbOperations;
    private readonly BotKeyboards _botKeyboards;

    public BotSettingsMenu(ITelegramBotClient botClient,
        UserDbOperations userDbOperations, BotKeyboards botKeyboards)
    {
        _botClient = botClient;
        _userDbOperations = userDbOperations;
        _botKeyboards = botKeyboards;
    }

    public async Task Display(Message message)
    {
        if (!await CheckIfRegistered(message)) return;

        await _botClient.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId);

        await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Choose menu item from a keyboard bellow.", replyMarkup: _botKeyboards.SettingsKeyboard());
    }

    private async Task<bool> CheckIfRegistered(Message message)
    {
        if (message.From.Id == null) return false;
        var user = await _userDbOperations.RetrieveTrelloUser((int)message.From.Id);
        if (user == null)
        {
            await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Looks like you haven't registered yet. Type /register first and follow registration process");

            return false;
        }

        return true;
    }

    public async Task CloseMenu(Message message)
    {
        if (!await CheckIfRegistered(message)) return;

        var chatId = message.Chat.Id;
        
        await _botClient.DeleteMessageAsync(chatId: chatId, messageId: message.MessageId);
        await _botClient.SendTextMessageAsync(chatId: chatId, text: "Back to work.",
            replyMarkup: _botKeyboards.MainKeyboard());
    }
}