using System.Globalization;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DigitArenaBot.Services;

public class TrackerCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private readonly IPersistanceService _persistanceService;
    private readonly List<ulong> _allowedChannels;
    private readonly IConfigurationRoot _config;

    public TrackerCommands(DiscordSocketClient client, InteractionService commands, IServiceProvider services, IPersistanceService persistanceService, IConfigurationRoot config)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _persistanceService = persistanceService;
        _config = config;
        _allowedChannels = _config.GetSection("AllowedChannels").Get<List<ulong>>();
    }
    
    [SlashCommand("cum", "cum")]
    public async Task Cum(string description)
    {
        if(!_allowedChannels.Contains(Context.Channel.Id))
        {
            await RespondAsync("Toto jde jen v aréně.");
            return;
        }

        if (description.Length > 256)
        {
            await RespondAsync("Max delka je 256");
            return;
        }

        await _persistanceService.SaveCumRecord(Context.User.Id, description);

        var embed = new EmbedBuilder();

        embed.Title = $"{Context.User.Username} cumnul.";
        embed.Description = $"**Description:**{description}";
        embed.ImageUrl =
            "https://gallery.lajtkep.dev/resources/e0969df67fb50172ec7a16e8a85b8b91beae6ed94a4a570a064afa7e9963033f.gif";

        await RespondAsync(null, embed: embed.Build(), allowedMentions: AllowedMentions.None);
    }
    
    [SlashCommand("cum-record", "xxx")]
    public async Task CumRecord(SocketGuildUser user)
    {
        if(!_allowedChannels.Contains(Context.Channel.Id))
        {
            await RespondAsync("Toto jde jen v aréně.");
            return;
        }

        var records = await _persistanceService.GetCumRecords(user.Id, 10);

        var embed = new EmbedBuilder();

        embed.Title = "Cum record";
        CultureInfo cs = new CultureInfo("cs-CZ");
        
        foreach (var cumRecord in records)
        {
            embed.Fields.Add(new EmbedFieldBuilder()
            {
                Name = cumRecord.Timestamp.ToString("U", cs),
                Value = cumRecord.Description,
                IsInline = false
            });
        }

        await RespondAsync(null, embed: embed.Build());
    }
    
    [SlashCommand("coomerboard", "yyy")]
    public async Task CoomerLeaderboard()
    {
        if(!_allowedChannels.Contains(Context.Channel.Id))
        {
            await RespondAsync("Toto jde jen v aréně.");
            return;
        }

        var records = await _persistanceService.GetCumLeaderboard(10);

        var embed = new EmbedBuilder();

        embed.Title = "Coomer leaderboard";
        
        foreach (var cumRecord in records)
        {
            embed.Description += $"<@{cumRecord.Id}> - {cumRecord.Count} \n";
        }

        await RespondAsync(null, embed: embed.Build(), allowedMentions: AllowedMentions.None);
    }
}