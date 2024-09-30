using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using DigitArenaBot.Classes.Reminder;

namespace DigitArenaBot.Services
{
    public class TimeService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public static bool isTomokopostingActivated = false;
        public System.Threading.Timer? _timer;

        private List<DiscordReminder> _eventTimes = new ();

        public TimeService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            Console.WriteLine("Initialization start");
            _client = client;
            _commands = commands;
            _services = services;
            Console.WriteLine("Initialization end");
        }

        public async Task RegisterEvent( DiscordReminder reminder)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(20);

            var eventEnd = reminder.remindAt;
            Console.WriteLine($"REGISTERED {eventEnd.ToUniversalTime()} at {DateTime.UtcNow.ToUniversalTime()}");
            _eventTimes.Add(reminder);
            
            if (_timer != null) return;
            
            _timer = new System.Threading.Timer(async (e) =>
            {
                Console.WriteLine($"{DateTime.Now} TICK EVENTS:{_eventTimes.Count}");
                
                var events = _eventTimes.Where(x => x.remindAt <= DateTime.UtcNow);
                _eventTimes = _eventTimes.Where(x => x.remindAt > DateTime.UtcNow).ToList();

                foreach (var UPPER in events)
                {
                    await Remind(UPPER);
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

        private async Task Remind(DiscordReminder reminder)
        {
            var builder = new EmbedBuilder();
            
            //builder.ImageUrl =
                //$"https://gallery.lajtkep.dev/api/files/getRandomFile.php?tag=tomoko_kuroki&seed={Random.Shared.Next()}";
             builder.ImageUrl = "https://gallery.lajtkep.dev/resources/6349ef8739bbee72df12326fa2793d3acc7e952cf213c4992163c6910391964f.jpg";

            var escapedMessage = reminder.message.Replace("@", "(at)");
            await _client.GetGuild(reminder.ServerId).GetTextChannel(reminder.ChannelId)
                .SendMessageAsync($"Hej <@{reminder.UserId}> měla jsem ti připomenout \"{escapedMessage}\"", embed: builder.Build(), allowedMentions:AllowedMentions.All);
        }
    }
}
