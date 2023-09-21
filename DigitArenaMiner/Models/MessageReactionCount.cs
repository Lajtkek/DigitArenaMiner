using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Models;

public class MessageReactionCount
{
    [Key]
    public ulong IdMessage { get; set; }
    public ulong IdSender { get; set; }
    
    [MaxLength(32)]
    public string EmoteIdentifier { get; set; }
    
    public int Count { get; set; }
}