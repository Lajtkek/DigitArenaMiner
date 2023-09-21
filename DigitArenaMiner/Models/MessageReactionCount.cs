using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Models;

[PrimaryKey("Id")]
public class MessageReactionCount
{
    public ulong Id { get; set; }
    
    public ulong IdMessage { get; set; }
    public ulong IdSender { get; set; }
    
    [MaxLength(32)]
    public string EmoteIdentifier { get; set; }
}