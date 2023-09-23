using System.Collections.Generic;
using System.Threading.Tasks;
using DigitArenaBot.Classes;
using DigitArenaBot.Models;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DigitArenaBot.Services;

public interface IPersistanceService
{
    public Task ArchiveMessage(ulong messageId);
    public Task<bool> IsMessageArchived(ulong messageId);

    public Task<MessageReactionCount?> GetMessageReactions(ulong messageId, MineableEmote emote);
    public Task ArchiveMessageReactions(ulong messageId, SocketUser user, MineableEmote emote, int count);
    
    
    public Task<List<LeaderboardItem>> Get(MineableEmote emote);
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
    }

    public async Task<MessageReactionCount?> GetMessageReactions(ulong messageId, MineableEmote emote)
    {
        return await _context.MessageReactionCounts.FirstOrDefaultAsync(x => x.IdMessage == messageId && x.EmoteIdentifier == emote.EmoteIdentifier);
    }

    public async Task ArchiveMessageReactions(ulong messageId, SocketUser user, MineableEmote emote, int count)
    {
        var reactions = await GetMessageReactions(messageId, emote);

        if (reactions != null)
        {
            reactions.Count = count;
        }
        else
        {
            await _context.MessageReactionCounts.AddAsync(new MessageReactionCount()
            {
                Count = count,
                EmoteIdentifier = emote.EmoteIdentifier,
                IdMessage = messageId,
                IdSender = user.Id
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<LeaderboardItem>> Get(MineableEmote emote)
    {
        var a = await _context.MessageReactionCounts
            .Where(x => x.EmoteIdentifier == emote.EmoteIdentifier)
            .GroupBy(x => x.IdSender)
            .Select(g => new
            {
                UserId = g.Key,
                TotalCount = g.Sum(x => x.Count)
            })
            .OrderByDescending(x => x.TotalCount)
            .Take(5)
            .ToListAsync();
        
        return a.Select(x => new LeaderboardItem()
        {
            Count = x.TotalCount,
            Id = x.UserId,
        }).ToList();
    }
}