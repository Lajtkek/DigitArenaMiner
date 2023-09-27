using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Models;

[Index(nameof(UserId), nameof(ActionName))]
[PrimaryKey(nameof(UserId),nameof(ActionName))]
public class UserActionCount
{
    public ulong UserId { get; set; }
    
    [MaxLength(20)]
    public string ActionName { get; set; }
    public int Count { get; set; }
}