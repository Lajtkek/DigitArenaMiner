using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitArenaBot.Services
{
    public class TimeService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public static bool isTomokopostingActivated = false;
        public System.Threading.Timer _timer;

        public TimeService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(1);

            Console.WriteLine("start timer");
            _timer = new System.Threading.Timer(async (e) =>
            {
                Console.WriteLine($"{DateTime.Now} TICK");
                if (isTomokopostingActivated)
                {
                    await PostTomoko();
                }
            }, null, startTimeSpan, periodTimeSpan);
            
        }

        private async Task PostTomoko()
        {
            var builder = new EmbedBuilder();
            
            builder.ImageUrl =
                $"https://gallery.lajtkep.dev/api/files/getRandomFile.php?tag=tomoko_kuroki&seed={Random.Shared.Next()}";

            await _client.GetGuild(760863954607669269).GetTextChannel(1145463500085411910)
                .SendMessageAsync(null, embed: builder.Build());
        }
    }
}