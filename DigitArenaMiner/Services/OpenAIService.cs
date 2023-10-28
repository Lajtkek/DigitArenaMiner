using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Discord.Rest;

namespace DigitArenaBot.Services
{
    public class OpenAIService
    {
        private readonly DiscordSocketClient _client;


        private readonly List<string> _systemMessages = new List<string>()
        {
            "Don't mention you are language model. You should behave as Tomoko Kuroki, who is 23 year old girl who studies Applied informatics at VŠE. Thanks to erasmus+ exchange program.",
            "YOU ARE GIRL. Everytime someone mentions Tomoko in context it is you no one else.",
            "You love person with nick 'Lajtkek' (when adressing just use Lajtkek), 'Matthew' and czech name 'Matěj Lajtkep'",
            "Don't write too formally.",
            "You don't need to advance conversation all the time.",
            "In chat provided messages you will see '<name>:<message>' pattern. DO NOT REPLY IN SAME PATTERN this pattern is used only to give you context who said what.",
            "No need to have long answers, you can sometimes reply with just yes, no.",
            "YOU ARE TOMOKO REPLY ONLY AS TOMOKO IN PATTERN 'Tomoko':<reply>",
            "You shoul reply only to message with [MAIN]",
            "Do not reply with prefix [MAIN] reply only with prefix Tomoko:",
        };

        private readonly int _contextLength = 10;
        
        public ChatCompletionsOptions GenerateChatOptions()
        {
            var options =  new ChatCompletionsOptions();

            foreach (var message in _systemMessages)
            {
                options.Messages.Add(new ChatMessage(ChatRole.System, message));
            }

            return options;
        }

        public OpenAIClient CreateClient()
        {
            return new OpenAIClient("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");
        }

        public OpenAIService(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += async message =>
            {
                // Coal mine
                if(message.Author.Id == 1155178035046252555) return;
                
                
                if (message.Author.Id != 256114627794960384) {
                    if (message.Author is SocketGuildUser _user)
                    {
                        if (!_user.Roles.Select(x => x.Id).Contains<ulong>(1167920011629838476)) return;
                    }
                }


                var messageContentLowered = message.Content.ToLower();
                if (messageContentLowered.Contains("me") && messageContentLowered.Contains("on") &&  (messageContentLowered.Contains("left") || messageContentLowered.Contains("right")))
                {
                    var isLeft = message.Content.ToLower().Contains("left") ? "right" : "left";
                    await message.Channel.SendMessageAsync($"me on {isLeft}", messageReference: message.Reference);
                    return;
                }
                
                if (messageContentLowered.Equals("us"))
                {
                    await message.Channel.SendMessageAsync($"this is so us");
                    return;
                }
                
                if (messageContentLowered.Contains("me") && messageContentLowered.Contains("and") && messageContentLowered.Contains("who"))
                {
                    await message.Channel.SendMessageAsync($"me <:feelsWOWman:946051635610812456>", messageReference: message.Reference);
                    return;
                }
                

                
                var options = GenerateChatOptions();

                var messageContext = new List<IMessage>();
                
                var idMessageReply = message.Reference?.MessageId;
                if (idMessageReply != null)
                {
                    var replyMsg = await message.Channel.GetMessageAsync(idMessageReply.GetValueOrDefault(0).Value);
    
                    if(replyMsg == null) return;

                    if (replyMsg.Author.Id != 1155178035046252555 && !message.Content.ToLower().Contains("tomoko")) return;

                    messageContext.Add(replyMsg);
                    messageContext.Add(message);
                }

                if (messageContext.Count == 0 && (message.Content.ToLower().Contains("tomoko") || message.Content.ToLower().Contains("mokochi") || message.Content.ToLower().Contains("tomoker")))
                {
                    messageContext.Add(message);
                }
                
                if (messageContext.Any())
                {
                    var messageReply = await message.Channel.SendMessageAsync($"let me think", messageReference: new MessageReference(message.Id));
                    var lastMessages = GetLastMessageContext(messageContext[0].Channel);

                    foreach (var lastMessage in lastMessages)
                    {
                        if(messageContext.All(x => x.Id != lastMessage.Id)) messageContext.Add(lastMessage);
                    }
                    
                    foreach (var messageData in messageContext.OrderBy(x => x.CreatedAt))
                    {
                        // await messageData.AddReactionAsync(new Emoji("👁️"));
                        var isMain = messageData.Id == message.Id ? "[MAIN]" : "";
                        options.Messages.Add(new ChatMessage(ChatRole.User, $"{isMain}{messageData.Author.Username}:" + messageData.Content));
                    }
                    
                    OpenAIClient client = CreateClient();
                    
                    var response = await client.GetChatCompletionsAsync("gpt-3.5-turbo", options);


                    await messageReply.ModifyAsync(properties =>
                    {
                        properties.Content = string.Join(" ", response.Value.Choices.Select(x => x.Message.Content));
                    });
                }
            };
        }

        private List<IMessage> GetLastMessageContext(IMessageChannel channel)
        {
            return channel.GetMessagesAsync(_contextLength).FlattenAsync().GetAwaiter().GetResult().ToList();
        }
    }
}