﻿using System;
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
         ulong _testGuildId;

         IEnumerable<MineableEmote> _mineableEmotes;

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
         
            
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
                .AddSingleton(config).AddDbContext<DefaultDatabaseContext>(options => {}, ServiceLifetime.Singleton)
                .AddSingleton<IPersistanceService, PersistanceService>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();

           
        
    
            _client = services.GetRequiredService<DiscordSocketClient>();
            _commands = services.GetRequiredService<InteractionService>();
            _persistanceService =  services.GetRequiredService<IPersistanceService>();
            _config =  services.GetRequiredService<IConfigurationRoot>();
        

         _testGuildId = ulong.Parse(_config["TestGuildId"]);
         _mineableEmotes = _config.GetSection("MineableEmotes").Get<List<MineableEmote>>();
         
            _client.Log += LogAsync;
            _commands.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.ReactionAdded += HandleReactionAsync;
            
         
            
            await _client.LoginAsync(TokenType.Bot, _config["Token"]);
            await _client.StartAsync();
       
        async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var reacter = _client.GetUser(reaction.UserId);
            if (reacter.IsBot) return;

            var minedEmote = _mineableEmotes.FirstOrDefault(x => reaction.Emote.Name == x.Name);
            if (minedEmote != null)
            {
                var emoteName = minedEmote.Name;

                IEmote emote = minedEmote.Id != null ? Emote.Parse(minedEmote.Id) : new Emoji(minedEmote.Name);
                var emotes = await message.GetOrDownloadAsync().Result.GetReactionUsersAsync(emote, 1000).FlattenAsync();
                int reactionCount = emotes.Count();

                ulong messageId = message.Id;
                
                await _persistanceService.ArchiveMessageReactions(messageId, reacter, minedEmote, reactionCount);
                
                if (reactionCount >= minedEmote.Threshold)
                {
                    ulong channelId = minedEmote.ChannelId;
                    if (await _persistanceService.IsMessageArchived(messageId))
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
                    await _persistanceService.ArchiveMessage(messageId);
                }
            }
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
            Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
        }

        // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
        static bool IsDebug ( )
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }

         builder.Build();
         await Task.Delay(Timeout.Infinite);