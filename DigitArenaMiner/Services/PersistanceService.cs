using System.Collections.Generic;
using System.Threading.Tasks;
using DigitArenaBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Services;

public interface IPersistanceService
{
    public Task ArchiveMessage(ulong messageId);
    public Task<bool> IsMessageArchived(ulong messageId);
}

public class PersistanceService : IPersistanceService
{
    public DefaultDatabaseContext _context;

    public PersistanceService(DefaultDatabaseContext context)
    {
        _context = context;
    }
    
    public async Task ArchiveMessage(ulong messageId)
    {
        if (!await IsMessageArchived(messageId))
        {
            await _context.ArchivedMessages.AddAsync(new ArchivedMessages()
            {
                Id = messageId
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsMessageArchived(ulong messageId)
    {
        return await _context.ArchivedMessages.AnyAsync(x => x.Id == messageId);
        return true;
    }
}