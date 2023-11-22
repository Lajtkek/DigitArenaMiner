using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Models;

[Index(nameof(Id),nameof(Index), IsUnique = true)]
public class PollAnswer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid IdParent { get; set; }
    
    [ForeignKey("IdParent")]
    public PollQuestion Parent { get; set; }
    
    [System.ComponentModel.DataAnnotations.MaxLength(64)]
    public string Body { get; set; }
    
    [System.ComponentModel.DataAnnotations.MaxLength(4)]
    public int Index { get; set; }
}