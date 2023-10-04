using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitArenaBot.Classes;
using DigitArenaBot.Classes.Game;
using Discord.Commands;
using Discord.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql.Replication.TestDecoding;

namespace DigitArenaBot.Services
{
    // interation modules must be public and inherit from an IInterationModuleBase
    public class GameCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }
        private IConfigurationRoot _config;
        private CommandHandler _handler;
        private DiscordSocketClient _client;
        private IPersistanceService _persistanceService;
        private readonly List<MineableEmote> _mineableEmotes;
        private readonly MessageReactionService _messageReactionService;

        private List<ulong> _allowedChannels;

        private List<Question> _questions;

        // constructor injection is also a valid way to access the dependecies
        public GameCommands (CommandHandler handler, IConfigurationRoot config, DiscordSocketClient client, IPersistanceService persistanceService, MessageReactionService messageReactionService)
        {
            _handler = handler;
            _config = config;
            _persistanceService = persistanceService;
            _client = client;
            _mineableEmotes = _config.GetSection("MineableEmotes").Get<List<MineableEmote>>();
            _messageReactionService = messageReactionService;
            
            _allowedChannels = _config.GetSection("AllowedChannels").Get<List<ulong>>();
            
            var userActions = _config.GetSection("UserActions").Get<List<UserAction>>();

            _questions = new List<Question>()
            {
                new Question()
                {
                    Title = "Best anime waifu",
                    Options = new List<string>() { "Tomoko", "Konata", "Natsuki" },
                    RightOptionIndex = 0
                },
                new Question()
                {
                    Title = "Best game",
                    Options = new List<string>() { "Tomoko", "placeholder" }
                }
            };
            
            _client.ButtonExecuted += async (component) =>
            {
                var builder = new EmbedBuilder();
                builder.ImageUrl =
                    "https://gallery.lajtkep.dev/resources/20543adff049ce884e9296ffe327a47b8c8a9906cded39a839c678019e803020.png";
                builder.Title = "Tadááááá";
                builder.Description = $"Zmáčkl ho {component.User.Username}";

                if (component.Data.CustomId == "Tomoko")
                {
                    
                    await component.RespondAsync("", embed: builder.Build());
                }
                else
                {
                    await component.RespondAsync($"{component.User.Username} vyber Tomoko.");
                }
               
            };
        }
        
        [SlashCommand("spawn-test-button", "Projede všechny zrávy a uloží emotes")]
         public async Task IndexChannel()
         {
             var index = Random.Shared.Next(0, _questions.Count - 1);
             var question = _questions[index];
             var builder = new ComponentBuilder();
             
             foreach (var answer in question.Options)
             {
                 builder.WithButton(answer, answer == "Tomoko" ? "Tomoko" : index + answer);
             }
             
             await RespondAsync("Zmáčkni ho", components: builder.Build());
             
         }
        
    }
}