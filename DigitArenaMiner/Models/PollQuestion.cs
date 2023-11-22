using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Models;

public class PollQuestion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public ulong CreatorId { get; set; }
    
    [MaxLength(64)]
    public string Title { get; set; }

    [MaxLength(2048)] public string Description { get; set; } = "";
    
    public virtual List<PollAnswer> Answers { get; set; }
}