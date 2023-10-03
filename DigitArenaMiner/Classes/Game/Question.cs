namespace DigitArenaBot.Classes.Game;

public class Question
{
    public string Title { get; set; }
    public List<string> Options { get; set; }
    public short RightOptionIndex { get; set; }
}