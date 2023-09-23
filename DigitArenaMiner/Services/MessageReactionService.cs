using DigitArenaBot.Classes;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;

namespace DigitArenaBot.Services;

public class MessageReactionService
{
    private readonly IConfigurationRoot _config;
    private readonly List<MineableEmote> _mineableEmotes;
    private readonly IPersistanceService _persistanceService;
    private readonly DiscordSocketClient _client;

    public MessageReactionService (CommandHandler handler, IConfigurationRoot config, DiscordSocketClient client, IPersistanceService persistanceService)
    {
        _config = config;
        _client = client;
        _persistanceService = persistanceService;
        _mineableEmotes = _config.GetSection("MineableEmotes").Get<List<MineableEmote>>();
    }
    
    public async Task OnMessageReaction(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        var minedEmote = _mineableEmotes.FirstOrDefault(x => reaction.Emote.Name == x.Name);
        
        if (minedEmote != null)
        {
            await OnEmoteDetected(message.Value, minedEmote);
        }
    }

    public async Task OnMessageReindex(IMessage message)
    {
        var reacions = message.Reactions;

        Console.WriteLine($"Reindexuju msg {message.Id} created at {message.CreatedAt}");
        foreach (var keyValuePair in reacions)
        {
           
            var minedEmote = _mineableEmotes.FirstOrDefault(x => keyValuePair.Key.Name == x.Name);

            if (minedEmote != null)
            {
               
                Console.WriteLine(minedEmote);
                await OnEmoteDetected(message, minedEmote);
            }
        }
    }

    public async Task OnEmoteDetected(IMessage message, MineableEmote minedEmote)
    {
        IEmote emote = minedEmote.Id != null ? Emote.Parse(minedEmote.Id) : new Emoji(minedEmote.Name);
        
        var emotes = await message.GetReactionUsersAsync(emote, 1000).FlattenAsync();
        int reactionCount = emotes.Count();

        ulong messageId = message.Id;
            
        await _persistanceService.ArchiveMessageReactions(messageId, message.Author.Id, minedEmote, reactionCount);
            
        if (reactionCount >= minedEmote.Threshold)
        {
            ulong channelId = minedEmote.ChannelId;
            if (await _persistanceService.IsMessageArchived(messageId))
            {
                return;
            }
                
            var chnl = _client.GetChannel(channelId) as IMessageChannel;
            if (chnl == null)
            {
                return;
            }

            var messageData = message.Content;
            var append = "\n";
            if (message.Attachments.Count > 0)
            {
                foreach (var messageDataAttachment in message.Attachments)
                {
                    append += messageDataAttachment.Url + "\n";
                }
            }

            var titleMessage = minedEmote.Message.Replace("{username}", "<@" + message.Author.Id + ">");
            var maxChars = 2000;
                
            var reply =  $"{message.GetJumpUrl()}\n" + titleMessage + "\n" + "{M}" + "\n" + append;

            var cutCopy = message.Content.Replace("@", "(at)").Substring(0, Math.Min(maxChars - reply.Length, message.Content.Length));

            reply = reply.Replace("{M}", cutCopy);

            
            await chnl.SendMessageAsync(reply);
            await _persistanceService.ArchiveMessage(messageId);
        }
    }
}