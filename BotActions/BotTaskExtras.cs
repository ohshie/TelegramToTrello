using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotActions;

public class BotTaskAdditions
{
    private DbOperations _dbOperation = new DbOperations();
    private TrelloOperations _trelloOperations = new TrelloOperations();
    private CreatingTaskDbOperations _creatingTaskDbOperations = new CreatingTaskDbOperations();
    
    private static ITelegramBotClient BotClient { get; set; }

    public BotTaskAdditions(ITelegramBotClient botClient)
    {
        BotClient = botClient;
    }

    public async Task ChoosingATaskToAddExtras(Message message)
    {
        TTTTask task = await _dbOperation.RetrieveUserTask((int)message.From.Id);
        if (task == null)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like there is no task created, " +
                                                       $"please create it first by typing \"/newtask name of the task\".",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        if (task.TaskId == "")
        {
            await BotClient.SendTextMessageAsync(text: "Looks like you haven't pushed your task yet.\n" +
                                                       "Please choose board and table for it and then use \"push\" to publish it to trello",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        if (message.Text.StartsWith("/desc")) await AddDescriptionToTask(message);

        if (message.Text.StartsWith("/part")) await ChoosingParticipants(message);
       
        if (message.Text.StartsWith("/name")) await AddPartiticantToTask(message);
        
        if (message.Text.StartsWith("/date")) await AddDateToTask(message);

        if (message.Text.StartsWith("/taskset")) await CompleteTask(message);
    }
    
    private async Task AddDescriptionToTask(Message message)
    {
        int telegramId = (int)message.From.Id;
        string trelloTaskDesc = message.Text.Substring("/desc".Length).Trim();
        Console.WriteLine($"{trelloTaskDesc}");

        if (trelloTaskDesc.StartsWith("@TelToTrelBot"))
        {
            trelloTaskDesc = trelloTaskDesc.Substring("@teltotrelbot".Length).Trim();
        }
        
        TTTTask task = await _dbOperation.RetrieveUserTask(telegramId);
        if (task == null)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like there is no task created already creating a task, " +
                                                       $"please create it first by typing \"/newtask name of the task\".",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        if (string.IsNullOrEmpty(trelloTaskDesc))
        {
            await BotClient.SendTextMessageAsync(text: "Please manually type \"/desc *description of your task*\".",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        await _creatingTaskDbOperations.AddDescriptionToTask(task, trelloTaskDesc);

        await _trelloOperations.PushTaskDescriptionToTrello(task);
    }

    private async Task ChoosingParticipants(Message message)
    {
        int telegramId = (int)message.From.Id;
        string participant = message.Text.Substring("/part".Length).Trim();
        Console.WriteLine($"{participant}");
        
        TTTTask task = await _dbOperation.RetrieveUserTask(telegramId);

        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardParticipants(task);
        
        await BotClient.SendTextMessageAsync(text: "choose participant from a list",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    private ReplyKeyboardMarkup KeyboardParticipants(TTTTask task)
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();
        
        BotDbContext dbContext = new BotDbContext();
        {
            TrelloUserBoard taskBoard = dbContext.TrelloUserBoards
                .Include(tub => tub.UsersOnBoards)
                .FirstOrDefault(tub => tub.TrelloBoardId == task.BoardId);
        
            if (taskBoard != null)
            {
                keyboardButtonsList.Add(new KeyboardButton[] {new KeyboardButton("/name press this when done")});
                foreach (var user in taskBoard.UsersOnBoards)
                {
                    keyboardButtonsList.Add(new KeyboardButton[] {new KeyboardButton($"/name {user.Name}")});
                }
            }
        
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
            {
                ResizeKeyboard = true,
                Selective = true
            };
        
            return replyKeyboardMarkup;
        }
    }

    private async Task AddPartiticantToTask(Message message)
    {
        int telegramId = (int)message.From.Id;
        string participantName = message.Text.Substring("/desc".Length).Trim();
        Console.WriteLine($"{participantName}");

        if (participantName == "press this when done")
        {
            await BotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "All participants added\n" +
                      "If you haven't added description or date do it now\n" +
                      "/desc | /date" ,
                      replyMarkup: new ReplyKeyboardRemove(),
                replyToMessageId: message.MessageId);
            return;
        }

        TTTTask task = await _dbOperation.RetrieveUserTask(telegramId);

        bool userFoundOnBoard = await _creatingTaskDbOperations.AddParticipantToTask(task, participantName);
        if (!userFoundOnBoard)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose name from keyboard menu.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        await _trelloOperations.PushTaskParticipantToTrello(task);
    }

    private async Task CompleteTask(Message message)
    {
        int telegramId = (int)message.From.Id;

        bool taskCleared = await _dbOperation.ClearTask(telegramId);
        if (taskCleared)
        {
            await BotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId,
                text: "All done, you can create new task now",
                replyMarkup: new ReplyKeyboardRemove());
        }
    }
    private async Task AddDateToTask(Message message)
    {
        int telegramId = (int)message.From.Id;
        string date = message.Text.Substring("/desc".Length).Trim();
        Console.WriteLine($"{date}");

        date = dateConverter(date);
        if (date == null)
        {
            await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        TTTTask task = await _dbOperation.RetrieveUserTask(telegramId);

        _creatingTaskDbOperations.AddDateToTask(task, date);

        _trelloOperations.PushDateToTrello(task);
        
        
    }

    private string dateConverter(string date)
    {
        DateTime properDate;
        DateTime.TryParseExact(date, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None,
            out properDate);
        if (properDate < DateTime.Today) return null;
       
        if (DateTime.TryParseExact(date, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out properDate))
        {
            string trelloDate = properDate.ToString("o");
            return trelloDate;
        }
        
        return null;
    }
}