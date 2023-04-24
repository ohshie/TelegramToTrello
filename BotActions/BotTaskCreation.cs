using System.Globalization;
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
                                                       "click on \"/register\" ",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        // step 1
        if (message.Text.StartsWith("/newtask")) await NewTaskBoard(message);
        if (message.Text.StartsWith("/board")) await NewTaskBoardSelection(message);
        
        // step 2
        if (message.Text.StartsWith("/list")) await NewTaskTableSelection(message);
        
        // step 3
        if (message.Text.StartsWith("/tag")) await NewTaskChanelTagSelection(message);
        
        // step 4
        if (message.Text.StartsWith("/push")) await PushTaskToTrello(message);
        
        //  step 5
        if (message.Text.StartsWith("/desc")) await GetDescriptionFromUser(message);
        
        // step 6
        if (message.Text.StartsWith("/part")) await ChoosingParticipants(message);
        if (message.Text.StartsWith("/name")) await AddParticipantToTask(message);
        
        // step 7 
        if (message.Text.StartsWith("/date")) await AskUserForADate(message);
    }
    
    // step 1 getting a board for new task
    private async Task NewTaskBoard(Message message)
    {
        // getting telegram id from message received
        int telegramId = (int)message.From.Id;
        
        // fetching user from db according to his telegram id
        RegisteredUsers? trelloUser = await _dbOperation.RetrieveTrelloUser(telegramId);
        
        // creating blank new entry in db with just telegram id.
        bool taskInProgress = await _creatingTaskDbOperations.AddTaskToDb(trelloUser.TelegramId);
        if (taskInProgress)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like you already creating a task.\n" +
                                                       $"Please finish it by following bor instructions first.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        // creating bot keyboard with user boards
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
            await BotClient.SendTextMessageAsync(text: "Ooops, something went wrong." +
                                                       $"Please start with /newtask",
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

        await BotClient.SendTextMessageAsync(text: "Choose trello list",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    // step 2 getting a table/list for a task on selected board
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
        
        bool listExist = await _creatingTaskDbOperations.AddTableToTask(telegramId, listName);
        if (!listExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose table name from keyboard menu.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardTagChoice();

        await BotClient.SendTextMessageAsync(text: "choose channel tag",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    // step 3 getting chanel tag for new task.
    
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

        await _creatingTaskDbOperations.AddPlaceholderName(telegramId);
        
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: message.Chat.Id,
            replyMarkup: new ReplyKeyboardRemove(),
            replyToMessageId: message.MessageId);
    }
    
    // strep 4 adding name to task
    public async Task AddNameToTask(TTTTask task, Message message)
    {
        if (message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id,
                replyToMessageId: message.MessageId,
                text: $"Task name should not start with \"/\"\n" +
                      $"Please type a new name for a task");
            return;
        }
        
        await _creatingTaskDbOperations.SetTaskName(task, message.Text);
                
        await BotClient.SendTextMessageAsync(message.Chat.Id,
            replyToMessageId: message.MessageId,
            text: $"Task name succesfully set to: {message.Text}\n" +
                  $"Press /desc when ready to add description.");
    }
    
    // step 5 adding description to the task   
    private async Task GetDescriptionFromUser(Message message)
    {
        int telegramId = (int)message.From.Id;

        TTTTask task = await _dbOperation.RetrieveUserTask(telegramId);
        if (task == null)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like no task is created yet.\n" +
                                                       $"please create it first by clicking /newtask",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        if (!task.NameSet) return;

        await _creatingTaskDbOperations.AddPlaceholderDescription(telegramId);
        
        await BotClient.SendTextMessageAsync(text: "Now please type description of your task in the next message.",
            chatId: message.Chat.Id,
            replyMarkup: new ReplyKeyboardRemove(),
            replyToMessageId: message.MessageId);
    }

    public async Task AddDescriptionToTask(TTTTask task, Message message)
    {
        if (message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id,
                replyToMessageId: message.MessageId,
                text: $"Task description should not start with \"/\"\n" +
                      $"Please type a new description for a task");
            return;
        }
        
        await _creatingTaskDbOperations.SetDescription(task, message.Text);

        await BotClient.SendTextMessageAsync(message.Chat.Id,
            replyToMessageId: message.MessageId,
            text: $"Task description successfully added\n" +
                  $"Press /part when ready to add participants");
    }
    
    // step 6 adding participants
    private async Task ChoosingParticipants(Message message)
    {
        int telegramId = (int)message.From.Id;
        string participant = message.Text.Substring("/part".Length).Trim();
        Console.WriteLine($"{participant}");
        
        TTTTask task = await _dbOperation.RetrieveUserTask(telegramId);

        ReplyKeyboardMarkup replyKeyboardMarkup = await KeyboardParticipants(task);
        
        await BotClient.SendTextMessageAsync(text: "choose participant from a list",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    private async Task<ReplyKeyboardMarkup> KeyboardParticipants(TTTTask task)
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();

        Boards taskBoards = await _dbOperation.RetrieveBoards(task.Id, task.BoardId);

        if (taskBoards != null)
        {
            keyboardButtonsList.Add(new KeyboardButton[] {new KeyboardButton("/name press this when done")});
            foreach (var user in taskBoards.UsersOnBoards.Select(uob => uob))
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

    private async Task AddParticipantToTask(Message message)
    {
        int telegramId = (int)message.From.Id;
        string participantName = message.Text.Substring("/part".Length).Trim();
        Console.WriteLine($"{participantName}");

        if (participantName == "press this when done")
        {
            await BotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "All participants added\n" +
                      "All is left is to add a due date\n" +
                      "Press /date when ready",
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
        }
    }
    
    // step 7 add date
    private async Task AskUserForADate(Message message)
    {
        int telegramId = (int)message.From.Id;
        string date = message.Text.Substring("/date".Length).Trim();
        Console.WriteLine($"{date}");

        TTTTask task = await _dbOperation.RetrieveUserTask(telegramId);
        if (task == null)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like there is no task created yet." +
                                                       $"Please create it first by clickling /newtask",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        if (!task.DescSet) return;
        
        await _creatingTaskDbOperations.AddPlaceholderDate(telegramId);
    
        await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
            chatId: message.Chat.Id,
            replyToMessageId: message.MessageId);
    }

    public async Task AddDateToTask(TTTTask task, Message message)
    {
        if (message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id,
                replyToMessageId: message.MessageId,
                text: $"Task date should not start with \"/\"\n" +
                      $"Please type a new date for a task.");
            return;
        }
        
        string possibleDate = dateConverter(message.Text);
        if (possibleDate == null)
        {
            await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        await _creatingTaskDbOperations.AddDateToTask(task, possibleDate);

        await DisplayCurrentTaskInfo(message, task);
    }

    private async Task DisplayCurrentTaskInfo(Message message, TTTTask task)
    {
        string boardName = await _dbOperation.BoardIdToName(task.BoardId);
        
        await BotClient.SendTextMessageAsync(text: "Lets review current task:\n\n" +
                                                   $"Task name: [{task.Tag}] {task.TaskName}\n" +
                                                   $"On board: {boardName}\n"+
                                                   $"Description: {task.TaskDesc}\n"+
                                                   $"Participants: {task.TaskPartName}\n"+
                                                   $"Due date: {DateTime.Parse(task.Date)}\n\n" +
                                                   $"If everything is correct press /push to post this task to trello\n",
            chatId: message.Chat.Id,
            replyToMessageId: message.MessageId);
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
        
        await _trelloOperations.PushTaskToTrello(userCreatedTask);
    }
}