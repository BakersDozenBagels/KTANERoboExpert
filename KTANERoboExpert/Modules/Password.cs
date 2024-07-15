using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class Password : RoboExpertModule
{
    public override string Name => "Password";
    public override string Help => "[6 letters] next [6 letters]";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(NATO.ToArray()), 6, 6) + "next" + new GrammarBuilder(new Choices(NATO.ToArray()), 6, 6));

    public override void Select()
    {
        Speak("Go on password columns 1 and 3");
    }

    public override void ProcessCommand(string command)
    {
        var parts = command.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]);
        var first = parts.Take(6).ToArray();
        var third = parts.Skip(7).ToArray();
        Speak(_passwords.Where(p => first.Contains(p[0]) && third.Contains(p[2])).Conjoin(", ", ", or "));
        ExitSubmenu();
    }

    private static readonly IReadOnlyCollection<string> _passwords = [
        "about",
        "after",
        "again",
        "below",
        "could",
        "every",
        "first",
        "found",
        "great",
        "house",
        "large",
        "learn",
        "never",
        "other",
        "place",
        "plant",
        "point",
        "right",
        "small",
        "sound",
        "spell",
        "still",
        "study",
        "their",
        "there",
        "these",
        "thing",
        "think",
        "three",
        "water",
        "where",
        "which",
        "world",
        "would",
        "write"
    ];
}
