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
        public System.Threading.Timer? _timer;

        private List<DateTime> _eventTimes = new List<DateTime>();

        public TimeService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            Console.WriteLine("init start");
            _client = client;
            _commands = commands;
            _services = services;
            Console.WriteLine("init end");
        }

        public async Task RegisterEvent(DateTime time)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(1);

            Console.WriteLine($"REGISTERED {time.ToUniversalTime()} at {DateTime.UtcNow.ToUniversalTime()}");
            _eventTimes.Add(time);
            
            if (_timer != null) return;
            
            _timer = new System.Threading.Timer(async (e) =>
            {
                Console.WriteLine($"{DateTime.Now} TICK EVENTS:{_eventTimes.Count}");
                
                var events = _eventTimes.Where(x => x <= DateTime.UtcNow);
                _eventTimes = _eventTimes.Where(x => x > DateTime.UtcNow).ToList();

                foreach (var UPPER in events)
                {
                    await PostTomoko();
                }

                if (_eventTimes.Count == 0)
                {
                    var timer = _timer;
                    _timer = null;
                    if(timer != null)
                    await timer.DisposeAsync();
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