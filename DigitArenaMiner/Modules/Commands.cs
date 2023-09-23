using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitArenaBot.Classes;
using Discord.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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

        // constructor injection is also a valid way to access the dependecies
        public ExampleCommands (CommandHandler handler, IConfigurationRoot config, DiscordSocketClient client, IPersistanceService persistanceService)
        {
            _handler = handler;
            _config = config;
            _persistanceService = persistanceService;
            _client = client;
            _mineableEmotes = _config.GetSection("MineableEmotes").Get<List<MineableEmote>>();
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
            Random r = new Random();
            var url = $"https://gallery.lajtkep.dev/api/files/getRandomFile.php?seed={r.NextInt64()}";
            await RespondAsync(url);
        }
        
        [SlashCommand("leaderboard", "Zobrazí")]
        public async Task Leaderboard(string emoteName)
        {
            var emote = _mineableEmotes.FirstOrDefault(x => x.Name == emoteName);
            if(emote == null) await RespondAsync($"ŠPATNEJ EMOTE dobrej emote(${string.Join(",",_mineableEmotes.Select(x => x.Name).ToList())})");
            
            await RespondAsync("Načítám data z data_22_09_2023.csv");
            
            var results = await _persistanceService.Get(emote);
            var response2 = results.Select(x => $"<@{x.Id}> má {x.Count}").ToList();

            await FollowupAsync($"{emote.EmoteIdentifier} LEADERBOARD \n" + string.Join("\n",response2));
        }
    }
}