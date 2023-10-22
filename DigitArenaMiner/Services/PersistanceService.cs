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
    public Task ArchiveMessageReactions(ulong messageId, ulong messageCreator, MineableEmote emote, int count);
    public Task<List<LeaderboardItem>> Get(MineableEmote emote);

    public Task UpdateUserActionCount(string actionName, ulong userId);
    public Task<UserActionCount?> GetUserActionCount(string actionName,ulong userId);
    public Task<List<LeaderboardItem>> GetTopUserActionCount(string actionName);

    public Task<bool> SaveCumRecord(ulong userId, string description);
    public Task<List<CumRecord>> GetCumRecords(ulong userId, int size);
    public Task<List<LeaderboardItem>> GetCumLeaderboard(int size);
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

    public async Task ArchiveMessageReactions(ulong messageId, ulong messageCreator, MineableEmote emote, int count)
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
                IdSender = messageCreator
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
            .Take(10)
            .ToListAsync();
        
        return a.Select(x => new LeaderboardItem()
        {
            Count = x.TotalCount,
            Id = x.UserId,
        }).ToList();
    }

    public async Task UpdateUserActionCount(string actionName, ulong userId)
    {
        var count = await GetUserActionCount(actionName, userId);

        if (count != null)
        {
            count.Count += 1;
        }
        else
        {
            await _context.UserActionCounts.AddAsync(new UserActionCount()
            {
                Count = 1,
                ActionName = actionName,
                UserId = userId
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserActionCount?> GetUserActionCount(string actionName, ulong userId)
    {
        return await _context.UserActionCounts.FirstOrDefaultAsync(
            x => x.ActionName == actionName && x.UserId == userId);
    }

    public async Task<List<LeaderboardItem>> GetTopUserActionCount(string actionName)
    {
        var a = await _context.UserActionCounts
            .Where(x => x.ActionName == actionName)
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalCount = g.Sum(x => x.Count)
            })
            .OrderByDescending(x => x.TotalCount)
            .Take(10)
            .ToListAsync();
        
        return a.Select(x => new LeaderboardItem()
        {
            Count = x.TotalCount,
            Id = x.UserId,
        }).ToList();
    }

    public async Task<bool> SaveCumRecord(ulong userId, string description)
    {
        var record = new CumRecord()
        {
            UserId = userId,
            Description = description
        };

        await _context.CumRecords.AddAsync(record);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<CumRecord>> GetCumRecords(ulong userId, int size = 10)
    {
        return await _context.CumRecords.Where(x => x.UserId == userId).OrderByDescending(x => x.Timestamp).Take(size).ToListAsync();
    }

    public async Task<List<LeaderboardItem>> GetCumLeaderboard(int size)
    {
        var a = await _context.CumRecords
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalCount = g.Count()
            })
            .OrderByDescending(x => x.TotalCount)
            .Take(10)
            .ToListAsync();
        
        return a.Select(x => new LeaderboardItem()
        {
            Count = x.TotalCount,
            Id = x.UserId,
        }).ToList();
    }
}