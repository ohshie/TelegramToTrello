using System.ComponentModel.DataAnnotations;

namespace TelegramToTrello;

public class DialogueStorage
{
    [Key]
    public int Id { get; set; }

    public int UserMessage { get; set; }
    public int BotMessage { get; set; }
    public string? MessageType { get; set; }
}