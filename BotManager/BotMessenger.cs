using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.Repositories;

namespace TelegramToTrello.BotManager;

public class BotMessenger
{
    private readonly DialogueStorageDbOperations _dialogueStorageDbOperations;
    private readonly ITelegramBotClient _botClient;

    public BotMessenger(DialogueStorageDbOperations dialogueStorageDbOperations, ITelegramBotClient botClient)
    {
        _dialogueStorageDbOperations = dialogueStorageDbOperations;
        _botClient = botClient;
    }

    public async Task SendMessage(int chatId, string text)
    {
        var message = await _botClient.SendTextMessageAsync(chatId: chatId, text: text);
        await _dialogueStorageDbOperations.SaveBotMessage(chatId, message.MessageId);
    }
    
    public async Task<Message> SendMessage(int chatId, string text, InlineKeyboardMarkup replyKeyboardMarkup)
    {
        var message = await _botClient.SendTextMessageAsync(chatId: chatId, 
            text: text, 
            replyMarkup: replyKeyboardMarkup);
        await _dialogueStorageDbOperations.SaveBotMessage(chatId, message.MessageId);

        return message;
    }
    
    public async Task<Message> SendMessage(int chatId, string text, ReplyKeyboardMarkup replyKeyboardMarkup)
    {
        var message = await _botClient.SendTextMessageAsync(chatId: chatId, 
            text: text,
            replyMarkup: replyKeyboardMarkup);
        await _dialogueStorageDbOperations.SaveBotMessage(chatId, message.MessageId);

        return message;
    }

    public async Task UpdateMessage(int chatId, int messageId, string text)
    {
        var message = await _botClient.EditMessageTextAsync(text: text, 
            messageId: messageId, 
            chatId: chatId);
        await _dialogueStorageDbOperations.SaveBotMessage(chatId, message.MessageId);
    }

    public async Task UpdateMessage(int chatId, int messageId, string text, 
        InlineKeyboardMarkup keyboardMarkup)
    {
        var message = await _botClient.EditMessageTextAsync(text: text, 
            messageId: messageId, 
            chatId: chatId,
            replyMarkup: keyboardMarkup);
        
        await _dialogueStorageDbOperations.SaveBotMessage(chatId, message.MessageId);
    }

    public async Task RemoveMessage(int chatId, int messageId)
    {
        try
        {
            await _botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to delete message: {messageId} in {chatId}.\n" +
                              $"more info:\n" +
                              $"{e}");
        }
    }

    public async Task RemoveLastBotMessage(int chatId)
    {
        try
        {
            var messageId = await _dialogueStorageDbOperations.Retrieve(chatId);
            await _botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId.BotMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to delete message in {chatId}.\n" +
                              $"more info:\n" +
                              $"{e}");
        }
    }
}