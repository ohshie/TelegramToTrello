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

    public async Task InitialTaskCreator(Message message)
    {
        int telegramId = (int)message.From.Id;
        RegisteredUsers? trelloUser = await _dbOperation.RetrieveTrelloUser(telegramId);
        if (trelloUser == null)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like you are not registered yet. " +
                                                       "Type \"/register\" with your trello username",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        if (message.Text.StartsWith("/newtask")) await AddNewTaskToDb(message);
        
        if (message.Text.StartsWith("/tag")) await NewTaskChanelTagSelection(message);
        
        if (message.Text.StartsWith("/board")) await NewTaskBoardSelection(message);

        if (message.Text.StartsWith("/list")) await NewTaskTableSelection(message);
        
        if (message.Text.StartsWith("/push")) await PushTaskToTrello(message);
    }

    private async Task AddNewTaskToDb(Message? message)
    {
        int telegramId = (int)message.From.Id;
        string newTaskName = message.Text.Substring("/newtask".Length).Trim();
        Console.WriteLine(newTaskName);
        if (newTaskName.StartsWith("@TelToTrelBot"))
        {
            await BotClient.SendTextMessageAsync(text: "Dont click on \"/newtask\"\n" +
                                                       $"Just type \"/newtask (name of the task)\"",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        if (string.IsNullOrEmpty(newTaskName))
        {
            await BotClient.SendTextMessageAsync(text: "Task name must not be empty. Try again",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        bool taskInProgress = await _creatingTaskDbOperations.AddTaskToDb(newTaskName, telegramId);
        if (taskInProgress)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like you already creating a task," +
                                                       $"please finish it first by choosing a board.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardTagChoice();

        await BotClient.SendTextMessageAsync(text: "choose channel tag",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    public async Task DiscardTask(Message message)
    {
        int telegramId = (int)message.From.Id;
        string newTaskName = message.Text.Substring("/newtask".Length).Trim();
        Console.WriteLine(newTaskName);
        
        
    }
    private ReplyKeyboardMarkup KeyboardTagChoice()
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();

        foreach (var tag in Enum.GetValues(typeof(ChanelTags)))
        {
            keyboardButtonsList.Add(new KeyboardButton[] { new KeyboardButton($"/tag {tag}") });
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true,
            Selective = true
        };

        return replyKeyboardMarkup;
    }

    private async Task NewTaskChanelTagSelection(Message message)
    {
        int telegramId = (int)message.From.Id;
        string tag = message.Text.Substring("/tag".Length).Trim();

        bool noTaskExist = await _dbOperation.CheckIfUserAlreadyCreatingTask(telegramId);
        if (!noTaskExist)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like there is no task created." +
                                                       $"please create it first by typing \"/newtask name of the task\".",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        if (!(Enum.TryParse(typeof(ChanelTags), tag, true, out _)))
        {
            await BotClient.SendTextMessageAsync(text: "Please choose tag from keyboard menu.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        Console.WriteLine(tag);

        await _creatingTaskDbOperations.AddTagToTask(telegramId, tag);

        RegisteredUsers? trelloUser = await _dbOperation.RetrieveTrelloUser(telegramId);

        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardBoardChoice(trelloUser);

        await BotClient.SendTextMessageAsync(text: "choose trello board",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    private ReplyKeyboardMarkup KeyboardBoardChoice(RegisteredUsers? trelloUser)
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();
        
        foreach (var board in trelloUser.UsersBoards.Select(ub => ub.Boards))
        {
            keyboardButtonsList.Add(new KeyboardButton[] { new KeyboardButton($"/board {board.BoardName}") });
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true,
            Selective = true
        };

        return replyKeyboardMarkup;
    }

    private async Task NewTaskBoardSelection(Message message)
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

        RegisteredUsers? trelloUser = await _dbOperation.RetrieveTrelloUser(telegramId);

        ReplyKeyboardMarkup replyKeyboardMarkup = await KeyboardTableChoice(trelloUser, boardName);

        await BotClient.SendTextMessageAsync(text: "choose trello board",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    private async Task<ReplyKeyboardMarkup> KeyboardTableChoice(RegisteredUsers trelloUser, string selectedBoardName)
    {
        Boards selectedBoard = await _dbOperation.RetrieveBoards(trelloUser.TelegramId, selectedBoardName);

            List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();

        foreach (var table in selectedBoard.Tables)
        {
            keyboardButtonsList.Add(new KeyboardButton[] { new KeyboardButton($"/list {table.Name}") });
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true,
            Selective = true
        };

        return replyKeyboardMarkup;
    }

    private async Task NewTaskTableSelection(Message message)
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

    private async Task PushTaskToTrello(Message message)
    {
        int telegramId = (int)message.From.Id;

        TTTTask userCreatedTask = await _dbOperation.RetrieveUserTask(telegramId);
        if (userCreatedTask == null)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like there is no task created." +
                                                       $"please create it first by typing \"/newtask name of the task\".",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        if (string.IsNullOrEmpty(userCreatedTask.BoardId) | string.IsNullOrEmpty(userCreatedTask.ListId) |
            string.IsNullOrEmpty(userCreatedTask.Tag))
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id,
                text: "You are pushing tag to early, please follow bot commands.",
                replyToMessageId: message.MessageId);
            return;
        }

        string taskId = await _trelloOperations.PushTaskToTrello(userCreatedTask);
        if (string.IsNullOrEmpty(taskId)) return;

        await _dbOperation.AddTaskIdToCreatedTasks(telegramId, taskId);

        await BotClient.SendTextMessageAsync(message.Chat.Id,
            text: "Task created.\n" +
                  "You can add task description with \"/desc your description\"\n" +
                  "Add participants to that task with \"/part\"\n" +
                  "And add due date with \"/date\"",
            replyToMessageId: message.MessageId);
    }
}