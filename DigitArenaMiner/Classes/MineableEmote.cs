namespace DigitArenaBot.Classes;

public class MineableEmote
{
    public string? Id { get; set; }
    public string Name { get; set; }
    public string Message { get; set; }
    public int Threshold { get; set; }
    public ulong ChannelId { get; set; }
}