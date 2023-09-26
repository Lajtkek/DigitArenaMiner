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
    
    public DynamicCommandService(IConfigurationRoot config, DiscordSocketClient client)
    {
        _config = config;
        _client = client;
        _userActions = _config.GetSection("UserActions").Get<List<UserAction>>();
    }

    public async Task RegisterDynamicCommands()
    {
        foreach (var userAction in _userActions)
        {
            await RegisterUserAction(userAction);
        }
        
        _client.InteractionCreated += async (interaction) =>
        {
            if (interaction is SocketSlashCommand slashCommand)
            {
                if (_userActions.Any(x => x.Name == slashCommand.CommandName))
                {
                    await HandleUserAction(slashCommand);
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
             
        await context.RespondAsync(msg, embeds: new []{embed.Build()});
    }
}