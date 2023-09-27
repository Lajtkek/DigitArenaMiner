using DigitArenaBot.Classes;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DigitArenaBot.Services;

public class DynamicCommandService
{
    private readonly IConfigurationRoot _config;
    private readonly DiscordSocketClient _client;
    private readonly List<UserAction> _userActions;
    private readonly IPersistanceService _persistanceService;
    
    public DynamicCommandService(IConfigurationRoot config, DiscordSocketClient client, IPersistanceService persistanceService)
    {
        _config = config;
        _client = client;
        _persistanceService = persistanceService;
        _userActions = _config.GetSection("UserActions").Get<List<UserAction>>();
    }

    public async Task RegisterDynamicCommands()
    {
        foreach (var userAction in _userActions)
        {
            await RegisterUserAction(userAction);
        }

        await RegisterUserActionLeaderboard();
        
        _client.InteractionCreated += async (interaction) =>
        {
            if (interaction is SocketSlashCommand slashCommand)
            {
                if (_userActions.Any(x => x.Name == slashCommand.CommandName))
                {
                    await HandleUserAction(slashCommand);
                }else if (slashCommand.CommandName == "useraction-leaderboard")
                {
                    await HandleUserActionLeaderboard(slashCommand);
                }
            }
        };
    }

    public async Task RegisterUserAction(UserAction action)
    {
        var guildCommand = new SlashCommandBuilder()
            .WithName(action.Name)
            .WithDescription("description")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("User for interaction")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.User)
            );
            
        try
        {
            await _client.Rest.CreateGlobalCommand(guildCommand.Build());
        }
        catch(ApplicationCommandException exception)
        {
            Console.WriteLine(exception.Message);
        }
    }

    public async Task RegisterUserActionLeaderboard()
    {
        var options = new SlashCommandOptionBuilder()
            .WithName("action-name")
            .WithDescription("User for interaction")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.String);
            
        foreach (var userAction in _userActions)
        {
            options.AddChoice(userAction.Name, userAction.Name);
        }
            
        var guildCommand = new SlashCommandBuilder()
            .WithName("useraction-leaderboard")
            .WithDescription("zobrazi top lidi co dostali akci")
            .AddOption(options);
        
        try
        {
            await _client.Rest.CreateGlobalCommand(guildCommand.Build());
        }
        catch(ApplicationCommandException exception)
        {
            Console.WriteLine(exception.Message);
        }
    }

    public async Task HandleUserAction(SocketSlashCommand context)
    {
        var targetedUser = context.Data.Options.First().Value as SocketGuildUser;
        var userAction = _userActions.First(x => x.Name == context.CommandName);

        ulong id = context.User.Id;
        ulong channelId = context.Channel.Id;
       
        var channel = await _client.GetChannelAsync(channelId) as ISocketMessageChannel;
        
        if (channel == null)
        {
            await context.RespondAsync("Channel neexistuje");
            return;
        }

        var msg = userAction.Message.Replace("{reciever}", $"<@{targetedUser.Id}>").Replace("{giver}", $"<@{id}>");

        var embed = new EmbedBuilder();
        embed.ImageUrl = userAction.ImageUrl;

        await _persistanceService.UpdateUserActionCount(userAction.Name, targetedUser.Id);
        await context.RespondAsync(msg, embeds: new []{embed.Build()});
    }
    
    private async Task HandleUserActionLeaderboard(SocketSlashCommand context)
    {
        var actionName = context.Data.Options.First().Value as string;

        var result = await _persistanceService.GetTopUserActionCount(actionName);
        
        var embed = new EmbedBuilder();
        embed.Title = $"{actionName} leaderboard:";
        embed.Description = string.Join("\n", result.Select(x => $"<@{x.Id}> má {x.Count}"));
        await context.RespondAsync("", embeds: new []{embed.Build()});
    }
}