using Telegram.Bot;

namespace TelegramToTrello.BotManager;

public class MessageRemover
{
    private readonly ITelegramBotClient _botClient;

    public MessageRemover(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task Remove(long chatId, int messageId)
    {
        try
        {
            await _botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}