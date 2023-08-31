using System.ComponentModel.DataAnnotations;

namespace TelegramToTrello;

public class RegisteredUser
{
    [Key]
    public int TelegramId { get; set; }
    public string? TelegramName { get; set; }
    public string? TrelloToken { get; set; }
    public string? TrelloId { get; set; }
    public string? TrelloName { get; set; }
    public bool NotificationsEnabled { get; set; }

    public ICollection<Board>? Boards { get; set; }
}

public class Board
{
    [Key]
    public int Id { get; set; }
    public string? TrelloBoardId { get; set; }
    public string? BoardName { get; set; }

    public ICollection<Table>? Tables { get; set; }
    public ICollection<UsersOnBoard>? UsersOnBoards { get; set; }
    public ICollection<RegisteredUser>? Users { get; set; } = new List<RegisteredUser>();
}

public class Table
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? TableId { get; set; }
    public int BoardId { get; set; }
    
    public Board? TrelloUserBoard { get; set; }
}

public class UsersOnBoard
{
    [Key]
    public int Id { get; set; }
    public string? TrelloUserId { get; set; }
    public string? Name { get; set; }
    public int TrelloUserBoardId { get; set; }
    
    public Board? TrelloBoard { get; set; }
}

public class TTTTask
{
    [Key]
    public int Id { get; set; }
    public string? TrelloId { get; set; }
    public string? TaskName { get; set; }
    public string? Tag { get; set; }
    public string? TrelloBoardId { get; set; }
    public string? TrelloBoardName { get; set; }
    public string? ListId { get; set; }
    public string? TaskId { get; set; }
    public string? TaskDesc { get; set; }
    public string? TaskPartId { get; set; }
    public string? TaskPartName { get; set; }
    public string? Date { get; set; }
    public string? Attachments { get; set; }
    public bool WaitingForAttachment { get; set; }
    public bool InEditMode { get; set; }
    public int LastBotMessage { get; set; }
}

public class TaskNotification
{
    [Key]
    public string? TaskId { get; set; }
    public string? TaskBoardId { get; set; }
    public string? TaskBoard { get; set; }
    public string? TaskListId { get; set; }
    public string? TaskList { get; set; }
    public string? Due { get; set; }
    public string? Url { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string[]? Participants { get; set; }
    public int User { get; set; }
    public bool EditMode { get; set; }
    public bool NotificationSent { get; set; }
}

public class Template
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? TemplateName { get; set; }
    public string? BoardName { get; set; }
    public string? BoardId { get; set; }
    public string? ListId { get; set; }
    public string? TaskName { get; set; }
    public string? TaskDesc { get; set; }
    public bool Complete { get; set; }
}

