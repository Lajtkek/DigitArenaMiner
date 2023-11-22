using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using DigitArenaBot.Classes;
using Discord.Commands;
using Discord.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql.Replication.TestDecoding;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace DigitArenaBot.Services
{
    // interation modules must be public and inherit from an IInterationModuleBase
    public class ExampleCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }
        private IConfigurationRoot _config;
        private CommandHandler _handler;
        private DiscordSocketClient _client;
        private IPersistanceService _persistanceService;
        private readonly List<MineableEmote> _mineableEmotes;
        private readonly MessageReactionService _messageReactionService;
        private readonly TimeService _timeService;
        private readonly VideoDownloadService _videoDownloadService;
        private readonly HelperService _helperService;
        private readonly OpenAIService _openAiService;
        private readonly List<string> _allowedRepostUrls = new ();
        

        private List<ulong> _allowedChannels;

        // constructor injection is also a valid way to access the dependecies
        public ExampleCommands (CommandHandler handler, IConfigurationRoot config, DiscordSocketClient client, IPersistanceService persistanceService, MessageReactionService messageReactionService, TimeService timeService, VideoDownloadService videoDownloadService, HelperService helperService, OpenAIService openAiService)
        {
            _handler = handler;
            _config = config;
            _persistanceService = persistanceService;
            _client = client;
            _mineableEmotes = _config.GetSection("MineableEmotes").Get<List<MineableEmote>>();
            _messageReactionService = messageReactionService;
            _timeService = timeService;
            _videoDownloadService = videoDownloadService;
            _helperService = helperService;
            _openAiService = openAiService;

            _allowedChannels = _config.GetSection("AllowedChannels").Get<List<ulong>>();
            
            _allowedRepostUrls = _config.GetSection("AllowedUrls").Get<List<string>>();
                
            var userActions = _config.GetSection("UserActions").Get<List<UserAction>>();
        }


        // our first /command!
        [SlashCommand("get-emotes", "Získá všechny třízene emotes")]
        public async Task GetEmotes()
        {
            var replies = new List<string>();

            foreach (var mineableEmote in _mineableEmotes)
            {
                replies.Add((mineableEmote.Id ?? mineableEmote.Name) + " - Needed reacts: " + mineableEmote.Threshold);    
            }

            await RespondAsync(string.Join("\n", replies));
        }
        
        [SlashCommand("get-gem", "Random gem z gallerid")]
        public async Task GetGem()
        {
            string username = Context.Interaction.User.Username;
            ulong cChannelId = Context.Interaction.Channel.Id;

            if (!_allowedChannels.Contains(cChannelId))
            {
                await RespondAsync($"Gemy postuju jen do arény.");
                return;
            }
            
            Random r = new Random();
            var url = $"https://gallery.lajtkep.dev/api/files/getRandomFile.php?seed={r.NextInt64()}";
            
            var embedBuilder = new EmbedBuilder
            {
                Title = $"Tady máš gem {username}.",
                // Description = "This is an example of an embed with an image.",
                Color = Color.Blue // You can set the color of the embed here
            };

            // Add an image to the embed
            embedBuilder.ImageUrl = url;
            
            var embed = embedBuilder.Build();
            
            await RespondAsync(null, new Embed[]
            {
                embed
            });
        }
        
        
        
        // [SlashCommand("leaderboard", "Zobrazí")]
        // public async Task Leaderboard(string emoteName)
        // {    
        //     var emote = _mineableEmotes.FirstOrDefault(x => x.Name == emoteName);
        //     if (emote == null)
        //     {
        //         await RespondAsync($"Emote není registrován, registrované emoty jsou ({string.Join(",",_mineableEmotes.Select(x => x.Name).ToList())})");
        //         return;
        //     }
        //     
        //     await RespondAsync("Načítám data z data_22_09_2023.csv");
        //     
        //     var results = await _persistanceService.Get(emote);
        //     var response2 = results.Select(x => $"<@{x.Id}> má {x.Count}").ToList();
        //
        //     var embedBuilder = new EmbedBuilder
        //     {
        //         Title = $"{emote.EmoteIdentifier} Leaderboard ",
        //         Description = string.Join("\n",response2),
        //         Color = Color.Default // You can set the color of the embed here
        //     };
        //
        //     await FollowupAsync(null, embed: embedBuilder.Build(), allowedMentions: Discord.AllowedMentions.None);
        // }
        
        [SlashCommand("good-morning", "idk")]
        public async Task GoodMorning()
        {
            string username = Context.Interaction.User.Username;

            var embedBuilder = new EmbedBuilder
            {
                Title = $"Good morning {username}.",
                // Description = "This is an example of an embed with an image.",
                Color = Color.Default // You can set the color of the embed here
            };

            // Add an image to the embed
            embedBuilder.ImageUrl = "https://gallery.lajtkep.dev/resources/873eb6ac2ebe161ddc0f2303761a5f999a5421e802c578100318d6b390025d6d.jpg"; // Replace with the URL of the image you want to include

            // Build the embed
            var embed = embedBuilder.Build();
            
            await RespondAsync(
                $"", new []{ embed });
        }
        
        [SlashCommand("good-morning-urinals", "idk")]
        public async Task GoodMorning2()
        {
            string username = Context.Interaction.User.Username;

            var embedBuilder = new EmbedBuilder
            {
                Title = $"Good morning urinals.",
                Color = Color.Default, // You can set the color of the embed here,
            };

            // Add an image to the embed
            var url = "https://gallery.lajtkep.dev/resources/2daa01d812c8a4665fdd58564574adbb510b8123a69b2dc8b114adc811deb88e.mp4"; // Replace with the URL of the image you want to include

            await DeferAsync();
            using (var client = new System.Net.Http.HttpClient())
            {
                var videoBytes = await client.GetByteArrayAsync(url);

                await FollowupWithFileAsync(new MemoryStream(videoBytes), "video.mp4", "Good morning urinals!");
            }
        }
        
        [SlashCommand("index-channel", "Projede všechny zrávy a uloží emotes")]
         public async Task IndexChannel()
         {
             ulong id = Context.Interaction.User.Id;
             ulong channelId = Context.Interaction.Channel.Id;
             if (id != 256114627794960384)
             {
                 await RespondAsync("Může jen lajtkek :-)");
                 return;
             }
        
             var channel = await _client.GetChannelAsync(channelId) as ISocketMessageChannel;
        
             if (channel == null)
             {
                 await RespondAsync("Channel neexistuje");
                 return;
             }
             
             await RespondAsync("Začínám indexovat");
             await RecursiveMessageHandler(channel, null);
             await FollowupAsync("Doindexovano");
         }
        
         private async Task RecursiveMessageHandler(ISocketMessageChannel channel, IMessage? message)
         {
             if (message == null)
             {
                 var res = await channel.GetMessagesAsync(1).FlattenAsync();
                 message = res.First();
                 await _messageReactionService.OnMessageReindex(message);
             }
        
             var limit = 500;
             var messages = await channel.GetMessagesAsync(message.Id, Direction.Before, limit).FlattenAsync();
        
             var index = 1;
             foreach (var msg in messages)
             {
                 if(message.Id == msg.Id) continue;
                 
                 await _messageReactionService.OnMessageReindex(msg);
                 
                 if(index == limit)  await RecursiveMessageHandler(channel, msg);
                 index++;
             }
         }

         // [SlashCommand("toggle-tomokoposting", "togluju tomokoposting")]
         // private async Task ToggleTomokoPosting(string date)
         // {
         //     DateTime time;
         //     var parsedDate = DateTime.TryParse(date, out time);
         //
         //     if (!parsedDate)
         //     {
         //         await RespondAsync($"Zadej UTC datum blbečku.");
         //         return;
         //     }
         //
         //     await _timeService.RegisterEvent(time);
         //     await RespondAsync($"Registrován event na {time.ToUniversalTime()}");
         // }
         
         public enum VideoFormat {
             Best,
             Worst
         }
         
         [SlashCommand("repost", "stáhne a repostne video")]
         public async Task RepostVideo(string url, string autorText = "")
         {
             if(!await _helperService.IsUserPrivileged(Context.User))
             {
                 await RespondAsync("**Tento příkaz je pouze pro privilegované uživatele.**");
                 return;
             }

             if (!_allowedRepostUrls.Any(x => url.StartsWith(x)))
             {
                 await RespondAsync("**Toto url neni supported, if its legit tell lajtkek to add it to config**");
                 return;
             }

             if (url.Contains("\"") || url.Contains(";"))
             {
                 await RespondAsync("**Toto url je neplatné**");
                 return;
             }
             
             await DeferAsync();
             var message = await Context.Channel.SendMessageAsync($"Progress");
             
             try
             {
                 var videoUrl = await _videoDownloadService.DownloadVideo(url, onProgress: (progressString) =>
                 {
                     message.ModifyAsync((m) =>
                     {
                         m.Content = progressString;
                     });
                     
                     return "";
                 });
                 
                 using var videStream = await _videoDownloadService.GetVideoStream(videoUrl);
                 
                 try
                 {
                     await message.ModifyAsync((m) =>
                     {
                         m.Content = "Uploading to discord";
                     });
                     autorText = autorText == "" ? "" : $"{Context.User.Username}:{autorText} \n";
                     await FollowupWithFileAsync(videStream, "video.mp4", $"**Tady máš video kámo!**\n{autorText} Původní odkaz:<{url}>");
                     await message.DeleteAsync();
                 }
                 catch (Exception e)
                 {
                     await message.DeleteAsync();
                     await Context.Channel.SendMessageAsync($"Nastala výjimka při postování videa. ({e.Message})");
                 }

                 // delete video anyways
                 await _videoDownloadService.DeleteVideo(videoUrl);
             }
             catch (Exception e)
             {
                 Console.WriteLine(e.Data);
                 Console.WriteLine(e.Source);
                 Console.WriteLine(e.StackTrace);
                 await message.DeleteAsync();
                 await FollowupAsync($"Nastala exception: {e.Message}");
             }
         }

         // [SlashCommand("context", "Získá kontext o poslední")]
         // public async Task GetContext(int messageCount, string? messageToBot)
         // {
         //     if(!await _helperService.IsUserPrivileged(Context.User))
         //     {
         //         await RespondAsync("**Tento příkaz je pouze pro privilegované uživatele.**");
         //         return;
         //     }
         //     
         //     if (messageCount > 500)
         //     {
         //         await RespondAsync("Tolik zpráv nemůžeš.");
         //         return;
         //     }
         //
         //     await RespondAsync("Jdu si to přečíst. hehe");
         //     
         //     var messages = Context.Channel.GetMessagesAsync(messageCount).FlattenAsync().GetAwaiter().GetResult();
         //
         //     var options = new ChatCompletionsOptions();
         //     options.Messages.Add(new ChatMessage(ChatRole.System, "Don't mention you are language model. You should behave as Tomoko Kuroki, who is 23 year old girl"));
         //     
         //     options.Messages.Add(new ChatMessage(ChatRole.System, $"Create short summary of following chat:"));
         //
         //     if (!string.IsNullOrEmpty(messageToBot))
         //     {
         //         options.Messages.Add(new ChatMessage(ChatRole.System, messageToBot));
         //     }
         //     
         //     foreach (var message in messages)
         //     {
         //         var replyReference = message.Reference != null ? $"(Replying to message:{message.Reference.MessageId})" : "";
         //         options.Messages.Add(new ChatMessage(ChatRole.System, $"(MessageId:{message.Id}) Message by {message.Author.Username} {replyReference}: {message.Content}"));
         //     }
         //
         //     var client = _openAiService.CreateClient();
         //
         //     var result = await client.GetChatCompletionsAsync("gpt-3.5-turbo", options);
         //
         //     foreach (var valueChoice in result.Value.Choices)
         //     {
         //         await FollowupAsync(valueChoice.Message.Content);
         //     }
         // }
    }
    

}