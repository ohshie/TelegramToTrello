using System.ComponentModel.DataAnnotations;

namespace TelegramToTrello;

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