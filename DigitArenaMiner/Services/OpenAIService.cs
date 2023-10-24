using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;

namespace DigitArenaBot.Services
{
    public class OpenAIService
    {
        private readonly DiscordSocketClient _client;

        public OpenAIService(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += async message =>
            {
                Console.WriteLine("XXXX" + message.Content);
                if (message.Content.StartsWith("Tomoko"))
                {
                    await message.Channel.SendMessageAsync("Hello");
                }
            };
            
        }
    }
}