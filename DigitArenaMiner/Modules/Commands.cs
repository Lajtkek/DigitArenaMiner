using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitArenaBot.Classes;
using Microsoft.Extensions.Configuration;

namespace DigitArenaBot.Services
{
    // interation modules must be public and inherit from an IInterationModuleBase
    public class ExampleCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }
        private IConfigurationRoot _config;
        private CommandHandler _handler;
        private readonly List<MineableEmote> _mineableEmotes;

        // constructor injection is also a valid way to access the dependecies
        public ExampleCommands (CommandHandler handler, IConfigurationRoot config)
        {
            _handler = handler;
            _config = config;
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

            await RespondAsync(("LOOOOOOOOOOOOOOsing it AAAAAAAAAAAAAAAAAAA"));
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
        public async Task Leaderboard(string test)
        {
            await RespondAsync($"Emote {test}");
        }
    }
}