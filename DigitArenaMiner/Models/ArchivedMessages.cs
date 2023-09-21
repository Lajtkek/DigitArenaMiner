using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Models;

[PrimaryKey(("Id"))]
public class ArchivedMessages
{
    public ulong Id { get; set; }
}