namespace DigitArenaBot.Classes.Reminder;

public record DiscordReminder(DateTime remindAt, ulong ServerId, ulong ChannelId, ulong UserId, string message);