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
using DigitArenaBot.Models;
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
        
        private readonly char _fullChar = '\u2593';
        private readonly char _blankChar = '\u2591';
        private readonly int _barLength = 20;

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
                if (component.Data.CustomId.StartsWith("Answer-"))
                {
                    await component.DeferAsync();
                    await HandleAnswerReaction(component);
                    return;
                }
                
                var builder = new EmbedBuilder();
                builder.ImageUrl =
                    "https://gallery.lajtkep.dev/resources/20543adff049ce884e9296ffe327a47b8c8a9906cded39a839c678019e803020.png";
                builder.Title = "Tadááááá";
                builder.Description = $"Zmáčkl ho {component.User.Username}";

                if (component.Data.CustomId == "Tomoko")
                {
                    await component.RespondAsync("", embed: builder.Build());
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

         [SlashCommand("create-question", "Vytvoří otázku")]
         public async Task CreateQuestion([MaxLength(64)] string question, [MaxLength(64)]string answer, [MaxLength(64)]string answer2, [MaxLength(64)]string? answer3 = null, [MaxLength(64)]string? answer4 = null)
         {
             await DeferAsync();

             var answers = new List<string>()
             {
                 answer,
                 answer2
             };

             if (answer3 != null) answers.Add(answer3);
             if(answer4 != null) answers.Add(answer4);

             var createdQuestion = await _persistanceService.CreateQuestion(Context.User.Id, question, answers);

             var (embed, messageComponent) = CreateQuestionEmbedBuilder(createdQuestion); 
             
             await FollowupAsync(embed: embed, components: messageComponent);
         }

         public string GetOptoinBar(int index, Dictionary<int, List<UserPollAnswer>> userAnswers)
         {
             decimal total = 0;
             var wantedAnswers = userAnswers.GetValueOrDefault(index);
             foreach (var userAnswersKey in userAnswers.Keys)
             {
                 total += userAnswers[userAnswersKey].Count;
             }

             if (total == 0 || wantedAnswers == null) return new string(_blankChar, _barLength);

             var charsToFill =  (int) (Math.Round(wantedAnswers.Count / total * _barLength * 10) / 10);

             return new string(_fullChar, charsToFill) + new string(_blankChar, _barLength - charsToFill);
         }
         
         public (Embed, MessageComponent) CreateQuestionEmbedBuilder(PollQuestion question, Dictionary<int, List<UserPollAnswer>>? usuerAsnwers = null)
         {
             // display
             var displayEmbed = new EmbedBuilder();
             displayEmbed.Title = question.Title;
             foreach (var _answer in question.Answers)
             {
                 var value = usuerAsnwers == null ? new string(_blankChar, _barLength) : GetOptoinBar(_answer.Index, usuerAsnwers);
                 displayEmbed.Fields.Add(new EmbedFieldBuilder()
                 {
                     Name = _answer.Body,
                     Value =  value
                 });
             }
    
             //buttons
             var buttonBuilder = new ComponentBuilder();
             foreach (var answer in question.Answers)
             {
                 buttonBuilder.WithButton(answer.Body, $"Answer-{question.Id}={answer.Id}");
             }

             return (displayEmbed.Build(), buttonBuilder.Build());
         }

         public async Task HandleAnswerReaction(SocketMessageComponent component)
         {
             var guidStrings = component.Data.CustomId.Replace("Answer-", "").Split("=");
             
             var idQuestion = Guid.Parse(guidStrings[0]);
             var idAnswer = Guid.Parse(guidStrings[1]);
             var userId = component.User.Id;

             await _persistanceService.ChangeAnswer(userId, idQuestion, idAnswer);
             
             // update component
             var question = await _persistanceService.GetQuestion(idQuestion);
             var voteResults = await _persistanceService.GetQuestionAnswers(idQuestion);
             var (embed, messageComponent) = CreateQuestionEmbedBuilder(question, voteResults);

             
             await component.ModifyOriginalResponseAsync(x =>
             {
                 x.Embed = embed;
             });
         }
    }
}