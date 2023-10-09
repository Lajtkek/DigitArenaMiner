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
    public class HighFiveModule : InteractionModuleBase<SocketInteractionContext>
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
        public HighFiveModule (CommandHandler handler, IConfigurationRoot config, DiscordSocketClient client, IPersistanceService persistanceService, MessageReactionService messageReactionService)
        {
            _handler = handler;
            _config = config;
            _persistanceService = persistanceService;
            _client = client;
            _mineableEmotes = _config.GetSection("MineableEmotes").Get<List<MineableEmote>>();
            _messageReactionService = messageReactionService;
            
            _allowedChannels = _config.GetSection("AllowedChannels").Get<List<ulong>>();
            
           
            
            _client.ButtonExecuted += async (component) =>
            {
                var builder = new ComponentBuilder();
                
                builder.WithButton($"Placák už vyžral {component.User.Username}", "DISABLED", disabled:true);
                
                if (component.Data.CustomId.Contains("High-Five"))
                {
                    ulong idAsker;
                    ulong.TryParse(component.Data.CustomId.Replace("High-Five", ""), out idAsker);

                    if (component.GuildId == null) return;
                    
                    var asker = _client.GetGuild(component.GuildId.Value).GetUser(idAsker);
                    // await component.RespondAsync("", embed: builder.Build());
                    await component.UpdateAsync((a) =>
                    {
                        a.Content = "Plácnuto";
                        a.Components = builder.Build();
                    }, null);

                    var embedBuilder = new EmbedBuilder();
                    embedBuilder.Title = "Proběhnul placák!";
                    embedBuilder.Description = $"{asker.Username} a {component.User.Username} si plánculi";
                    embedBuilder.ImageUrl = "https://media.tenor.com/jib9YZw1YsIAAAAC/madagscar-penguins.gif";


                    await component.Channel.SendMessageAsync("", embed:embedBuilder.Build());
                    //await FollowupAsync("", embed: embedBuilder.Build());
                }
               
            };
        }
        
        [SlashCommand("high-five", "Projede všechny zrávy a uloží emotes")]
         public async Task IndexChannel()
         {
             var builder = new ComponentBuilder();

             builder.WithButton("Plácnout si", $"High-Five{Context.User.Id.ToString()}");
             
             var component = builder.Build();
             await RespondAsync($"{Context.User.Username} si chce plácnout", components:component );
         }
        
    }
}