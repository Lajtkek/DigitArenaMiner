using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DigitArenaBot.Services;
using System.Configuration;
using System.Linq;
using System.Threading;
using DigitArenaBot;
using DigitArenaBot.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;


// setup our fields we assign later
IConfigurationRoot _config;
DiscordSocketClient _client;
InteractionService _commands;
IPersistanceService _persistanceService;
MessageReactionService _messageReactionService;
DynamicCommandService _dynamicCommandService;
ulong _testGuildId;

IEnumerable<MineableEmote> _mineableEmotes;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "Railway_");

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var socketConfig = new DiscordSocketConfig()
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
};

var socketClient = new DiscordSocketClient(socketConfig);

var connectionString = config["ConnectionStrings:Db"];

var services = builder.Services
    .AddSingleton(socketClient)
    .AddSingleton<HelperService>()
    .AddSingleton(config).AddDbContext<DefaultDatabaseContext>(options => { }, ServiceLifetime.Singleton)
    .AddSingleton<IPersistanceService, PersistanceService>()
    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
    .AddSingleton<CommandHandler>()
    .AddSingleton<MessageReactionService>()
    .AddSingleton<TimeService>()
    .AddSingleton<DynamicCommandService>()
    .AddSingleton<VideoDownloadService>()
    .BuildServiceProvider();


_client = services.GetRequiredService<DiscordSocketClient>();
_commands = services.GetRequiredService<InteractionService>();
_persistanceService = services.GetRequiredService<IPersistanceService>();
_config = services.GetRequiredService<IConfigurationRoot>();
_messageReactionService = services.GetRequiredService<MessageReactionService>();
_dynamicCommandService = services.GetRequiredService<DynamicCommandService>();

_testGuildId = ulong.Parse(_config["TestGuildId"]);
_mineableEmotes = _config.GetSection("MineableEmotes").Get<List<MineableEmote>>();

_client.Log += LogAsync;
_commands.Log += LogAsync;
_client.Ready += ReadyAsync;
_client.ReactionAdded += HandleReactionAsync;

// await _commands.AddModuleAsync<ExampleCommands>(services);

await services.GetRequiredService<CommandHandler>().InitializeAsync();

await _client.LoginAsync(TokenType.Bot, _config["Token"]);
await _client.StartAsync();

async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel,
    SocketReaction reaction)
{
    var reacter = _client.GetUser(reaction.UserId);
    if (reacter.IsBot) return;

    await _messageReactionService.OnMessageReaction(message, channel, reaction);
}

Task LogAsync(LogMessage log)
{
    Console.WriteLine(log.ToString());
    return Task.CompletedTask;
}

async Task ReadyAsync()
{
   
    if (IsDebug())
    {
        // this is where you put the id of the test discord guild
        System.Console.WriteLine($"In debug mode, adding commands to {_testGuildId}...");
        await _commands.RegisterCommandsToGuildAsync(_testGuildId);
    }
    else
    {
        // this method will add commands globally, but can take around an hour
        await _commands.RegisterCommandsGloballyAsync(true);
    }

    await _dynamicCommandService.RegisterDynamicCommands();
    Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
}

// this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
static bool IsDebug()
{
#if DEBUG
    return true;
#else
                return false;
#endif
}

builder.Build();
await Task.Delay(Timeout.Infinite);