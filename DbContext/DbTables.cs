namespace TelegramToTrello;

public class RegisteredUser
{
    public int TelegramId { get; set; }
    public string TelegramName { get; set; } = string.Empty;
    public string TrelloToken { get; set; } = string.Empty;
    public string TrelloId { get; set; } = string.Empty;
    public string TrelloName { get; set; } = string.Empty;
    public bool NotificationsEnabled { get; set; }

    public ICollection<UsersBoards>? UsersBoards { get; set; }
}

public class Board
{
    public int Id { get; set; }
    public string TrelloBoardId { get; set; } = string.Empty;
    public string BoardName { get; set; } = string.Empty;
    public int TelegramId { get; set; }

    public ICollection<Table>? Tables { get; set; }
    public ICollection<UsersOnBoard>? UsersOnBoards { get; set; }
    public ICollection<UsersBoards>? UsersBoards { get; set; }
}

public class UsersBoards
{
    public int UserId { get; set; }
    public RegisteredUser? RegisteredUsers { get; set; }

    public int BoardId { get; set; }
    public Board? Boards { get; set; }
}

public class Table
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TableId { get; set; } = string.Empty;
    public int BoardId { get; set; }
    
    public Board? TrelloUserBoard { get; set; }
}

public class UsersOnBoard
{
    public int Id { get; set; }
    public string TrelloUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TrelloUserBoardId { get; set; }
    
    public Board? TrelloBoard { get; set; }
}

public class TTTTask
{
    public int Id { get; set; }
    public string TrelloId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string TrelloBoardId { get; set; } = string.Empty;
    public string TrelloBoardName { get; set; } = string.Empty;
    public string ListId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string TaskDesc { get; set; } = string.Empty;
    public string TaskPartId { get; set; } = string.Empty;
    public string TaskPartName { get; set; } = string.Empty;
    public string? Date { get; set; }
    public bool NameSet { get; set; }
    public bool DescSet { get; set; }
    public bool DateSet { get; set; }
}

public class TaskNotification
{
    public string Id { get; set; } = string.Empty;
    public string Due { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int User { get; set; }
}

