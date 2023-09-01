using System.ComponentModel.DataAnnotations;

namespace TelegramToTrello;

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