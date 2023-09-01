using System.ComponentModel.DataAnnotations;

namespace TelegramToTrello;

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