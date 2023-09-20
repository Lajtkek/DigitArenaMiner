using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitArenaBot.Services;

public interface IPersistanceService
{
    public Task SaveMessageSent(ulong messageId);
    public Task<bool> GetMessageSent(ulong messageId);
}

public class PersistanceService : IPersistanceService
{
    public List<ulong> _cache = new List<ulong>();
    
    public async Task SaveMessageSent(ulong messageId)
    {
        if (!await GetMessageSent(messageId))
        {
            _cache.Add(messageId);
        }
    }

    public async Task<bool> GetMessageSent(ulong messageId)
    {
        return _cache.Contains(messageId);
    }
}