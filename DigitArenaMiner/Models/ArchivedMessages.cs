using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Models;

[PrimaryKey(("Id"))]
public class ArchivedMessages
{
    public Guid Id { get; set; }
}