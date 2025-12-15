using System.Speech.Recognition;
using System.Diagnostics;

namespace KTANERoboExpert.Modules;

public partial class ColourFlash : RoboExpertModule
{
    public override string Name => "Colour Flash";
    public override string Help => "red in red, blue in green, ...";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("red", "yellow", "green", "blue", "magenta", "white").ToGrammarBuilder() + "in" + new Choices("red", "yellow", "green", "blue", "magenta", "white"), 8, 8));

    public override void ProcessCommand(string command)
    {
        var parts = command.Split(" ").Chunk(3).Select(ch => (Word: ch[0], Color: ch[2])).ToArray();
        var answer = Solve(parts);
        Speak($"Press {(answer.Yes ? "Yes" : "No")} on {answer.Item1.Index + 1}, {answer.Item1.Word} in {answer.Item1.Color}");
        ExitSubmenu();
    }

    private ((int Index, string Word, string Color), bool Yes) Solve((string Word, string Color)[] parts)
    {
        var data = parts.Select((p, i) => (Index: i, Word: p.Word, Color: p.Color));
        switch (parts[^1].Color)
        {
            case "red":
                if (parts.Count(p => p.Word == "green") >= 3)
                    return (data.First(t => t.Color == "green" || t.Word == "green"), Yes: true);
                if (parts.Count(p => p.Color == "blue") == 1)
                    return (data.First(t => t.Word == "magenta"), Yes: false);
                return (data.Last(t => t.Word == "white" || t.Color == "white"), Yes: true);
            case "yellow":
                if (parts.Any(p => p.Word == "blue" && p.Color == "green"))
                    return (data.First(t => t.Color == "green"), Yes: true);
                if (parts.Any(p => p.Word == "white" && (p.Color == "white" || p.Color == "red")))
                    return (data.Where(t => t.Color != t.Word).Skip(1).First(), Yes: true);
                var index = parts.Count(p => p.Word == "magenta" || p.Color == "magenta");
                return (data.Skip(index - 1).First(), Yes: false);
            case "green":
                if (Enumerable.Range(0, 7).Any(i => parts[i].Word == parts[i + 1].Word && parts[i].Color != parts[i + 1].Color))
                    return (data.Skip(4).First(), Yes: false);
                if (parts.Count(p => p.Word == "magenta") >= 3)
                    return (data.First(t => t.Word == "yellow" || t.Color == "yellow"), Yes: false);
                return (data.First(t => t.Word == t.Color), Yes: true);
            case "blue":
                if (parts.Count(p => p.Color != p.Word) >= 3)
                    return (data.First(t => t.Word != t.Color), Yes: true);
                if (parts.Any(p => p.Word == "red" && p.Color == "yellow" || p.Word == "yellow" && p.Color == "white"))
                    return (data.First(t => t.Word == "white" && t.Color == "red"), Yes: false);
                return (data.Last(t => t.Word == "green" || t.Color == "green"), Yes: true);
            case "magenta":
                if (Enumerable.Range(0, 7).Any(i => parts[i].Word != parts[i + 1].Word && parts[i].Color == parts[i + 1].Color))
                    return (data.Skip(2).First(), Yes: true);
                if (parts.Count(p => p.Word == "yellow") > parts.Count(p => p.Color == "blue"))
                    return (data.Last(t => t.Word == "yellow"), Yes: false);
                var color = parts[6].Word;
                return (data.First(t => t.Color == color), Yes: false);
            case "white":
                color = parts[2].Color;
                if (parts[3].Word == color || parts[4].Word == color)
                    return (data.First(t => t.Word == "blue" || t.Color == "blue"), Yes: false);
                if (parts.Any(p => p.Word == "yellow" && p.Color == "red"))
                    return (data.Last(t => t.Color == "blue"), Yes: true);
                return (data.First(), Yes: false);
            default:
                throw new UnreachableException();
        }
    }
}
