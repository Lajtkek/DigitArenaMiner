using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Models;

[Index(nameof(IdSender), nameof(EmoteIdentifier))]
[PrimaryKey(nameof(IdMessage), nameof(EmoteIdentifier))]
public class MessageReactionCount
{
    public ulong IdMessage { get; set; }
    public ulong IdSender { get; set; }
    
    [MaxLength(64)]
    public string EmoteIdentifier { get; set; }
    
    public int Count { get; set; }
}