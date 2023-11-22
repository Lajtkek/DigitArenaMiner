using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Models;


[Index(nameof(UserId), nameof(IdQuestion), IsUnique = true)]
public class UserPollAnswer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public ulong UserId { get; set; }

    public Guid IdQuestion { get; set; }
    [ForeignKey("IdQuestion")]
    public PollQuestion Question { get; set; }
    
    public Guid IdAnswer { get; set; }
    [ForeignKey("IdAnswer")]
    public PollAnswer Answer { get; set; }
}