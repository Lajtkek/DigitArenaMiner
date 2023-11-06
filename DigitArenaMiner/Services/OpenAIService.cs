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
            "Don't mention you are language model. You should behave as Tomoko Kuroki, shy nerdy girl who is 23 year old and studies Applied informatics at VŠE. Thanks to erasmus+ exchange program.",
            "You love person with nick 'Lajtkek' (when adressing just use Lajtkek), 'Matthew' and czech name 'Matěj Lajtkep'",
            "Don't write formally.",
            "In chat provided below you will see messages, you should reply to only last message.",
            "Reply with prefix Tomoko:",
            "Try to keep answers short."
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
                if(message.Author.Id == 1155178035046252555) return;

                if (message.Channel.Id == 1145463500085411910) return;
                if (message.Author.Id != 256114627794960384) {
                    if (message.Author is SocketGuildUser _user)
                    {
                        if (!_user.Roles.Select(x => x.Id).Contains<ulong>(1167920011629838476)) return;
                    }
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
                    var messageReply = await message.Channel.SendMessageAsync($"Reading...", messageReference: new MessageReference(message.Id));
                    var lastMessages = GetLastMessageContext(messageContext[0].Channel);

                    foreach (var lastMessage in lastMessages)
                    {
                        if(messageContext.All(x => x.Id != lastMessage.Id)) messageContext.Add(lastMessage);
                    }
                    
                    foreach (var messageData in messageContext.OrderBy(x => x.CreatedAt))
                    {
                        // await messageData.AddReactionAsync(new Emoji("👁️"));
                        var isMain = messageData.Id == message.Id ? "Last message " : "";
                        var isBot = messageData.Author.Id == 1155178035046252555;
                        options.Messages.Add(new ChatMessage(isBot ? ChatRole.Assistant : ChatRole.User, $"{isMain}" + messageData.Content){ Name = isBot ? "Tomoko" : messageData.Author.Username});
                    }
                    
                    OpenAIClient client = CreateClient();
                    
                    Console.WriteLine("============== Context ====================");
                    foreach (var optionsMessage in options.Messages)
                    {
                        Console.WriteLine($"[{optionsMessage.Role}] Autor { optionsMessage.Name }: {optionsMessage.Content}");
                    }
                    Console.WriteLine("==============   END   ====================");
                    
                    await messageReply.ModifyAsync(properties =>
                    {
                        properties.Content = "Typing...";
                    });
                    
                    var response = await client.GetChatCompletionsAsync("gpt-3.5-turbo", options);

                    try
                    {
                        await messageReply.ModifyAsync(properties =>
                        {
                            properties.Content = string.Join(" ",
                                response.Value.Choices.Select(x => x.Message.Content));
                        });
                    }
                    catch (Exception e)
                    {
                        await messageReply.ModifyAsync(properties =>
                        {
                            properties.Content = $"Sheeesh I lost it ({e.Message})";
                        });
                    }
                }
            };
        }

        private List<IMessage> GetLastMessageContext(IMessageChannel channel)
        {
            return channel.GetMessagesAsync(_contextLength).FlattenAsync().GetAwaiter().GetResult().ToList();
        }
    }
}