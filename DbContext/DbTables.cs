namespace TelegramToTrello;

public class RegisteredUser
{
    public int TelegramId { get; set; }
    public string TelegramName { get; set; }
    public string TrelloToken { get; set; }
    public string TrelloId { get; set; }
    public string TrelloName { get; set; }
    public bool NotificationsEnabled { get; set; }

    public ICollection<UsersBoards> UsersBoards { get; set; }
}

public class Board
{
    public int Id { get; set; }
    public string TrelloBoardId { get; set; }
    public string BoardName { get; set; }
    public int TelegramId { get; set; }

    public ICollection<Table> Tables { get; set; }
    public ICollection<UsersOnBoard> UsersOnBoards { get; set; }
    public ICollection<UsersBoards> UsersBoards { get; set; }
}

public class UsersBoards
{
    public int UserId { get; set; }
    public RegisteredUser RegisteredUsers { get; set; }

    public int BoardId { get; set; }
    public Board Boards { get; set; }
}

public class Table
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TableId { get; set; }
    public int BoardId { get; set; }
    
    public Board TrelloUserBoard { get; set; }
}

public class UsersOnBoard
{
    public int Id { get; set; }
    public string TrelloUserId { get; set; }
    public string Name { get; set; }
    public int TrelloUserBoardId { get; set; }
    
    public Board TrelloBoard { get; set; } 
}

public class TTTTask
{
    public int Id { get; set; }
    public string TrelloId { get; set; }
    public string TaskName { get; set; }
    public string Tag { get; set; }
    public string TrelloBoardId { get; set; }
    public string ListId { get; set; }
    public string TaskId { get; set; }
    public string TaskDesc { get; set; }
    public string TaskPartId { get; set; }
    public string TaskPartName { get; set; }
    public string? Date { get; set; }
    public bool NameSet { get; set; }
    public bool DescSet { get; set; }
    public bool DateSet { get; set; }
}

public class TaskNotification
{
    public string Id { get; set; }
    public string Due { get; set; }
    public string Url { get; set; }
    public string Name { get; set; }
    public int User { get; set; }
}

