using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DigitArenaBot.Services;
using DigitArenaBot.Classes;

namespace DigitArenaBot
{
    class Program
    {
        // setup our fields we assign later
        private IConfigurationRoot _config;
        private DiscordSocketClient _client;
        private InteractionService _commands;
        private IPersistanceService _persistanceService;
        private ulong _testGuildId;

        private IEnumerable<MineableEmote> _mineableEmotes;

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync(string[] args)
        {
            
        }

        public Program()
        {
  
        }

        public async Task MainAsync()
        {
            // call ConfigureServices to create the ServiceCollection/Provider for passing around the services
            using (var services = ConfigureServices())
            {
                // get the client and assign to client 
                // you get the services via GetRequiredService<T>
             
                var client = services.GetRequiredService<DiscordSocketClient>();
                var commands = services.GetRequiredService<InteractionService>();
                _persistanceService = services.GetRequiredService<IPersistanceService>();
                _config = services.GetRequiredService<IConfigurationRoot>();
                _client = client;
                _commands = commands;
                
                _testGuildId = ulong.Parse(_config["TestGuildId"]);

                _mineableEmotes = _config.GetSection("MineableEmotes").Get<List<MineableEmote>>();

                // setup logging and the ready event
                client.Log += LogAsync;
                commands.Log += LogAsync;
                client.Ready += ReadyAsync;
                client.ReactionAdded += HandleReactionAsync;

                // this is where we get the Token value from the configuration file, and start the bot
                await client.LoginAsync(TokenType.Bot, _config["Token"]);
                await client.StartAsync();

                // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
                await services.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (_client.GetUser(reaction.UserId).IsBot) return;

            var minedEmote = _mineableEmotes.FirstOrDefault(x => reaction.Emote.Name == x.Name);
            if (minedEmote != null)
            {
                var emoteName = minedEmote.Name;

                IEmote emote = minedEmote.Id != null ? Emote.Parse(minedEmote.Id) : new Emoji(minedEmote.Name);
                var emotes = await message.GetOrDownloadAsync().Result.GetReactionUsersAsync(emote, 1000).FlattenAsync();
                int reactionCount = emotes.Count(); 

                if (reactionCount >= minedEmote.Threshold)
                {
                    ulong channelId = minedEmote.ChannelId;
                    ulong messageId = message.Id;
                    if (await _persistanceService.GetMessageSent(messageId))
                    {
                        return;
                    }
                    
                    var chnl = _client.GetChannel(channelId) as IMessageChannel;
                    if (chnl == null)
                    {
                        return;
                    }

                    var messageData = await message.DownloadAsync();
                    var append = "\n";
                    if (messageData.Attachments.Count > 0)
                    {
                        foreach (var messageDataAttachment in messageData.Attachments)
                        {
                            append += messageDataAttachment.Url + "\n";
                        }
                    }

                    var titleMessage = minedEmote.Message.Replace("{username}", "<@" + messageData.Author.Id + ">");
                    var maxChars = 2000;
                    
                    var reply =  titleMessage+ "\n" + "{M}" + "\n" + append;

                    var cutCopy = messageData.Content.Replace("@", "(at)").Substring(0, Math.Min(maxChars - reply.Length, messageData.Content.Length));

                    reply = reply.Replace("{M}", cutCopy);
                    
                    await chnl.SendMessageAsync(reply);
                    await _persistanceService.SaveMessageSent(messageId);
                }
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
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
            Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
        }

        // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
        private ServiceProvider ConfigureServices()
        {
            // this returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using csharpi.Services;
            var socketConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            var idk = new DiscordSocketClient(socketConfig);
            
            //todoAdd binder
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var per = new PersistanceService();
            
            
            return new ServiceCollection()
                .AddSingleton(idk)
                .AddSingleton(config)
                .AddSingleton<IPersistanceService>(per)
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }

        static bool IsDebug ( )
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }
    }
}