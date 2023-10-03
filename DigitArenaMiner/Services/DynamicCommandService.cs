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
    private readonly List<Guru> _gurus;
    
    public DynamicCommandService(IConfigurationRoot config, DiscordSocketClient client)
    {
        _config = config;
        _client = client;
        _userActions = _config.GetSection("UserActions").Get<List<UserAction>>();
        _gurus = _config.GetSection("Gurus").Get<List<Guru>>();
    }

    public async Task RegisterDynamicCommands()
    {
        foreach (var userAction in _userActions)
        {
            await RegisterUserAction(userAction);
        }

        await RegisterGurus();
        
        _client.InteractionCreated += async (interaction) =>
        {
            if (interaction is SocketSlashCommand slashCommand)
            {
                if (_userActions.Any(x => x.Name == slashCommand.CommandName))
                {
                    await HandleUserAction(slashCommand);
                }else if (slashCommand.CommandName == "ask")
                {
                    await HandleAskCommand(slashCommand);
                }
            }
        };
    }

    private async Task RegisterGurus()
    {
        var guruPicker = new SlashCommandOptionBuilder()
            .WithName("guru")
            .WithDescription("Guru to ask")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.String);
        
        foreach (var guru in _gurus)
        {
            guruPicker.AddChoice(guru.Name, guru.Name);
        }
        
        var guildCommand = new SlashCommandBuilder()
            .WithName("ask")
            .WithDescription("Zeptej se někoho, kdo o životě ví více než ty.")
            .AddOption(guruPicker).AddOption(new SlashCommandOptionBuilder()
                .WithName("question")
                .WithRequired(true)
                .WithDescription("Question to ask").WithType(ApplicationCommandOptionType.String));
        
        try
        {
            await _client.Rest.CreateGlobalCommand(guildCommand.Build());
        }
        catch(ApplicationCommandException exception)
        {
            Console.WriteLine(exception.Message);
        }
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
    
    private async Task HandleAskCommand(SocketSlashCommand context)
    {
        var guruName = context.Data.Options.First().Value as string;
        var question = context.Data.Options.Last().Value as string;
        
        var guru = _gurus.First(x => x.Name == guruName);

        ulong id = context.User.Id;
        ulong channelId = context.Channel.Id;
       
        var channel = await _client.GetChannelAsync(channelId) as ISocketMessageChannel;
        
        if (channel == null)
        {
            await context.RespondAsync("Channel neexistuje");
            return;
        }

        var answerIndex = Random.Shared.Next(0, guru.Answers.Length -1);
        var answer = guru.Answers[answerIndex];
        var msg = guru.Message.Replace("{result}", $"**{answer}**");

        var embed = new EmbedBuilder();
        embed.ImageUrl = guru.ImageUrl;

        embed.Title = $"{context.User.Username} se zeptal {guruName}";


        embed.Fields = new List<EmbedFieldBuilder>()
        {
            new EmbedFieldBuilder()
            {
                Name = "Otázka:",
                Value = question
            },
            new EmbedFieldBuilder()
            {
                Name = "Odpověď:",
                Value = msg
            }
        };
        
             
        await context.RespondAsync(null, embeds: new []{embed.Build()});
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