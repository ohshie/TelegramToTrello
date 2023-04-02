using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotActions;

public class BotTaskCreation
{
    private DbOperations _dbOperation = new DbOperations();
    private CreatingTaskDbOperations _creatingTaskDbOperations = new CreatingTaskDbOperations();
    private TrelloOperations _trelloOperations = new TrelloOperations();

    private ITelegramBotClient BotClient { get; set; }

    public BotTaskCreation(ITelegramBotClient botClient)
    {
        BotClient = botClient;
    }

    public async Task AddNewTaskToDb(Message? message)
    {
        int telegramId = (int)message.From.Id;
        string newTaskName = message.Text.Substring("/newtask".Length).Trim();
        Console.WriteLine(newTaskName);
        if (newTaskName.StartsWith("@teltotrelbot"))
        {
            await BotClient.SendTextMessageAsync(text: "Dont click on \"/newtask\"\n" +
                                                       $"Just type \"/newtask (name of the task)\"",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        TrelloUser? trelloUser = await _dbOperation.RetrieveTrelloUser(telegramId);
        if (trelloUser == null)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like you are not registered yet. " +
                                                       "Type \"/register\" with your trello username",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        bool taskInProgress = await _creatingTaskDbOperations.AddTaskToDb(newTaskName, telegramId);
        if (taskInProgress)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like you already creating a task, " +
                                                       $"please finish it first by choosing a board.",
                                                chatId: message.Chat.Id,
                                                replyToMessageId: message.MessageId);
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardBoardChoice(trelloUser);

        await BotClient.SendTextMessageAsync(text: "choose trello board",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }
    
    private ReplyKeyboardMarkup KeyboardBoardChoice(TrelloUser? trelloUser)
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();
        
        foreach (var board in trelloUser.TrelloUserBoards)
        {
            keyboardButtonsList.Add(new KeyboardButton[] {new KeyboardButton($"/board {board.Name}")});
        }
        
        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true,
            Selective = true
        };
        
        return replyKeyboardMarkup;
    }
    
    public async Task NewTaskBoardSelection(Message message)
    {
        int telegramId = (int)message.From.Id;
        
        string boardName = message.Text.Substring("/board".Length).Trim();
        
        bool noTaskExist = await _dbOperation.CheckIfUserAlreadyCreatingTask(telegramId);
        if (!noTaskExist)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like there is no task created already creating a task, " +
                                                       $"please create it first by typing \"/newtask name of the task\".",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        Console.WriteLine(boardName);

        bool boardExist = await _creatingTaskDbOperations.AddBoardToTask(telegramId, boardName);
        if (!boardExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose board name from keyboard menu.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        TrelloUser? trelloUser = await _dbOperation.RetrieveTrelloUser(telegramId);
        
        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardTableChoice(trelloUser, boardName);

        await BotClient.SendTextMessageAsync(text: "choose trello board",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    private ReplyKeyboardMarkup KeyboardTableChoice(TrelloUser trelloUser, string selectedBoardName)
    {
        TrelloUserBoard selectedBoard = trelloUser.TrelloUserBoards.FirstOrDefault(board => board.Name == selectedBoardName);
        
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();
        
        foreach (var table in selectedBoard.TrelloBoardTables)
        {
            keyboardButtonsList.Add(new KeyboardButton[] {new KeyboardButton($"/list {table.Name}")});
        }
        
        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true,
            Selective = true
        };
        
        return replyKeyboardMarkup;
    }
    
    public async Task NewTaskTableSelection(Message message)
    {
        int telegramId = (int)message.From.Id;
        string listName = message.Text.Substring("/list".Length).Trim();

        bool noTaskExist = await _dbOperation.CheckIfUserAlreadyCreatingTask(telegramId);
        if (!noTaskExist)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like there is no task created already creating a task, " +
                                                       $"please create it first by typing \"/newtask name of the task\".",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        Console.WriteLine(listName);

        bool listExist = await _creatingTaskDbOperations.AddTableToTask(telegramId, listName);
        if (!listExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose table name from keyboard menu.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        await BotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "You are now ready to push your task with /push",
            replyMarkup: new ReplyKeyboardRemove(),
            replyToMessageId: message.MessageId);
    }

    public async Task PushTaskToTrello(Message message)
    {
        int telegramId = (int)message.From.Id;
        
        TTTTask userCreatedTask = await _dbOperation.RetrieveUserTask(telegramId);
        if (userCreatedTask == null) return;
        
        string taskId = await _trelloOperations.PushTaskToTrello(userCreatedTask);
        if (string.IsNullOrEmpty(taskId)) return;
        
        await _dbOperation.AddTaskIdToCreatedTasks(telegramId, taskId);
        
        await BotClient.SendTextMessageAsync(message.Chat.Id, 
            text:"Task created.\n" +
                 "You can add task description with \"/desc your description\"\n" +
                 "Add participants to that task with \"/part\"\n" +
                 "And add due date with \"/date\"",
            replyToMessageId: message.MessageId);
    }
}