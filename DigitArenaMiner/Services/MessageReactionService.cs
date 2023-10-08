using System.Net;
using DigitArenaBot.Classes;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using EmbedBuilder = Discord.EmbedBuilder;

namespace DigitArenaBot.Services;

public class MessageReactionService
{
    private readonly IConfigurationRoot _config;
    private readonly List<MineableEmote> _mineableEmotes;
    private readonly IPersistanceService _persistanceService;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;

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
            var dmsg = await message.DownloadAsync();
            await OnEmoteDetected(dmsg, minedEmote);
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
        
        Console.WriteLine(message.Reactions.Keys.Count());
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

            var titleMessage = minedEmote.Message.Replace("{username}", message.Author.Username);
            var maxChars = 2000;
            
            var embedBuilder = new EmbedBuilder
            {
                Title = titleMessage,
                Description = messageData,
                Color = Color.Default,
                Url = message.GetJumpUrl()
            };

            // await chnl.SendMessageAsync(embed: embedBuilder.Build(), allowedMentions: AllowedMentions.All);

            if (message.Attachments.Any())
            {
                var att = new List<FileAttachment>();
                var streamList = new List<Stream>();
                using (var client = new WebClient())
                {
                    foreach (var file in message.Attachments)
                    {

                        var content = await client.DownloadDataTaskAsync(file.Url);
                        streamList.Add(new MemoryStream((byte[])content));
                        att.Add(new FileAttachment(streamList.Last(), file.Filename, "kopie, kopie, kopie..."));
                    }
                }

            await chnl.SendFilesAsync(att, embed: embedBuilder.Build(), stickers: message.Stickers as ISticker[], allowedMentions: AllowedMentions.None);

                foreach (var stream in streamList)
                {
                    await stream.DisposeAsync();
                }
            }else
                await chnl.SendMessageAsync(messageData, embed: embedBuilder.Build(), stickers: message.Stickers as ISticker[], allowedMentions: AllowedMentions.None);
            
            await _persistanceService.ArchiveMessage(messageId);
        }
    }
}