using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Discord.Rest;
using Microsoft.Extensions.Configuration;

namespace DigitArenaBot.Services
{
    public class OpenAIService
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _configuration;


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
        private readonly string _gptToken;

        public ChatCompletionsOptions GenerateChatOptions()
        {
            var options = new ChatCompletionsOptions();

            foreach (var message in _systemMessages)
            {
                options.Messages.Add(new ChatMessage(ChatRole.System, message));
            }

            options.DeploymentName = "gpt-3.5-turbo";

            return options;
        }

        public OpenAIClient CreateClient()
        {
            return new OpenAIClient(_gptToken);
        }

        public OpenAIService(DiscordSocketClient client, IConfigurationRoot configuration)
        {
            _client = client;
            _configuration = configuration;
            _gptToken = Environment.GetEnvironmentVariable("GPT_TOKEN");
            
            _client.MessageReceived += async message =>
            {
                if (message.Author.Id == _client.CurrentUser.Id) return;

                if (message.Channel.Id == 1145463500085411910) return;
                if (message.Author.Id != 256114627794960384)
                {
                    if (message.Author is SocketGuildUser _user)
                    {
                        if (!_user.Roles.Select(x => x.Id).Contains<ulong>(1167920011629838476)) return;
                    }
                }

                if (message.Content.ToLower().Contains("tomoko look"))
                {
                    await OnMessageWithAttachment(message);
                    return;
                }

                var options = GenerateChatOptions();

                var messageContext = new List<IMessage>();

                var idMessageReply = message.Reference?.MessageId;
                if (idMessageReply != null)
                {
                    var replyMsg = await message.Channel.GetMessageAsync(idMessageReply.GetValueOrDefault(0).Value);

                    if (replyMsg == null) return;

                    if (replyMsg.Author.Id != _client.CurrentUser.Id &&
                        !message.Content.ToLower().Contains("tomoko")) return;

                    messageContext.Add(replyMsg);
                    messageContext.Add(message);
                }

                if (messageContext.Count == 0 && (message.Content.ToLower().Contains("tomoko") ||
                                                  message.Content.ToLower().Contains("mokochi") ||
                                                  message.Content.ToLower().Contains("tomoker")))
                {
                    messageContext.Add(message);
                }

                if (messageContext.Any())
                {
                    var messageReply = await message.Channel.SendMessageAsync($"Reading...",
                        messageReference: new MessageReference(message.Id));
                    var lastMessages = GetLastMessageContext(messageContext[0].Channel);

                    foreach (var lastMessage in lastMessages)
                    {
                        if (messageContext.All(x => x.Id != lastMessage.Id)) messageContext.Add(lastMessage);
                    }

                    foreach (var messageData in messageContext.OrderBy(x => x.CreatedAt))
                    {
                        // await messageData.AddReactionAsync(new Emoji("👁️"));
                        var isMain = messageData.Id == message.Id ? "Last message " : "";
                        var isBot = messageData.Author.Id == 1155178035046252555;
                        options.Messages.Add(
                            new ChatMessage(isBot ? ChatRole.Assistant : ChatRole.User,
                                    $"{isMain}" + messageData.Content)
                                { Name = isBot ? "Tomoko" : messageData.Author.Username });
                    }

                    OpenAIClient client = CreateClient();

                    Console.WriteLine("============== Context ====================");
                    foreach (var optionsMessage in options.Messages)
                    {
                        Console.WriteLine(
                            $"[{optionsMessage.Role}] Autor {optionsMessage.Name}: {optionsMessage.Content}");
                    }

                    Console.WriteLine("==============   END   ====================");

                    await messageReply.ModifyAsync(properties => { properties.Content = "Typing..."; });


                    try
                    {
                        var response = await client.GetChatCompletionsAsync(options);

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

        private async Task OnMessageWithAttachment(SocketMessage message)
        {
            var allowedContentTypes = new List<string>() { "image/jpeg", "image/png" };
            var imageAttachment = message.Attachments.FirstOrDefault(x => allowedContentTypes.Contains(x.ContentType));
            if (imageAttachment is null) return;

            string apiUrl = "https://api.openai.com/v1/chat/completions";

            var requestData = new Dictionary<string, object>
            {
                { "model", "gpt-4-vision-preview" },
                {
                    "messages", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "role", "system" },
                            { "content", "You are Tomoko Kuroki, character from series 'Watmote' and you are asistant. If you see image with two characters and question is if it is us write 'yes, so us' else reply normally" },
                        },
                        new Dictionary<string, object>
                        {
                            { "role", "user" },
                            {
                                "content", new List<Dictionary<string, object>>
                                {
                                    new Dictionary<string, object>
                                    {
                                        { "type", "text" },
                                        {
                                            "text",
                                            message.Content.Replace("tomoko look", "").Replace("Tomoko look", "")
                                        },
                                    },
                                    new Dictionary<string, object>
                                    {
                                        { "type", "image_url" },
                                        {
                                            "image_url", new Dictionary<string, object>
                                            {
                                                {
                                                    "url",
                                                    imageAttachment.Url
                                                },
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
                { "max_tokens", 200 },
            };

            using (var client = new HttpClient())
            {
                var requestContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8,
                    "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_gptToken}");
                Console.WriteLine($"Bearer {_gptToken}");

                var response = await client.PostAsync(apiUrl, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

                    if (responseObject.TryGetValue("choices", out var choices) && choices is JsonElement choicesElement)
                    {
                        var content = choicesElement[0].GetProperty("message").GetProperty("content").GetRawText();
                        await message.Channel.SendMessageAsync(content.Replace("\\\\","\\"), messageReference: new MessageReference(message.Id));
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }

        private List<IMessage> GetLastMessageContext(IMessageChannel channel)
        {
            return channel.GetMessagesAsync(_contextLength).FlattenAsync().GetAwaiter().GetResult().ToList();
        }
    }
}