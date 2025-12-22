using System.Speech.Recognition;

namespace KTANERoboExpert.Modules.Vanilla;

public class Keypad : RoboExpertModule
{
    public override string Name => "Keypad";
    public override string Help => "backwards c then euro then train tracks then controller";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(_symbolNames.Keys.ToArray()).ToGrammarBuilder() + "then", 3, 3) + new Choices(_symbolNames.Keys.ToArray()));

    public override void ProcessCommand(string command)
    {
        var symbols = command.Split(" then ").Select(x => _symbolNames[x]).ToArray();
        var col = _columns.FirstOrDefault(c => symbols.All(c.Contains));
        if (col == null)
        {
            Speak("Pardon?");
            return;
        }
        Speak(col.Where(symbols.Contains).Select(s => (Array.IndexOf(symbols, s) + 1).ToString()).Conjoin());
        ExitSubmenu();
        Solve();
    }

    private static readonly Dictionary<string, int> _symbolNames = new()
    {
        ["tennis racket"] = 0,
        ["lollipop"] = 0,
        ["alfa tango"] = 1,
        ["pyramid"] = 1,
        ["illuminati"] = 1,
        ["lambda"] = 2,
        ["half life"] = 2,
        ["lightning"] = 3,
        ["lightning bolt"] = 3,
        ["zig zag"] = 3,
        ["cat"] = 4,
        ["big yus"] = 4,
        ["squid knife"] = 4,
        ["cursive h"] = 5,
        ["reverse c"] = 6,
        ["backwards c"] = 6,
        ["backwards c dot"] = 6,
        ["reverse charlie"] = 6,
        ["backwards charlie"] = 6,
        ["backwards charlie dot"] = 6,
        ["euro"] = 7,
        ["backwards e"] = 7,
        ["backwards euro"] = 7,
        ["curly cue"] = 8,
        ["loop de loop"] = 8,
        ["cursive q"] = 8,
        ["hollow star"] = 9,
        ["empty star"] = 9,
        ["question mark"] = 10,
        ["upside down question mark"] = 10,
        ["copyright"] = 11,
        ["controller"] = 12,
        ["weird w"] = 12,
        ["eye"] = 12,
        ["back to back k's"] = 13,
        ["melted 3"] = 14,
        ["broken 3"] = 14,
        ["broken r"] = 14,
        ["flat 6"] = 15,
        ["6"] = 15,
        ["paragraph"] = 16,
        ["pilcrow"] = 16,
        ["bravo tango"] = 17,
        ["tango bravo"] = 17,
        ["smiley face"] = 18,
        ["smile"] = 18,
        ["psi"] = 19,
        ["trident"] = 19,
        ["charlie"] = 20,
        ["charlie dot"] = 20,
        ["c"] = 20,
        ["c dot"] = 20,
        ["alien 3"] = 21,
        ["weird 3"] = 21,
        ["snake"] = 21,
        ["filled star"] = 22,
        ["black star"] = 22,
        ["puzzle piece"] = 23,
        ["train tracks"] = 23,
        ["not equal"] = 23,
        ["a e"] = 24,
        ["a e ligature"] = 24,
        ["hieroglyphic h"] = 25,
        ["hieroglyphic n"] = 25,
        ["omega"] = 26,
    };
    private static readonly int[][] _columns =
    [
        [0, 1, 2, 3, 4, 5, 6],
        [7, 0, 6, 8, 9, 5, 10],
        [11, 12, 8, 13, 14, 2, 9],
        [15, 16, 17, 4, 13, 10, 18],
        [19, 18, 17, 20, 16, 21, 22],
        [15, 7, 23, 24, 19, 25, 26],
    ];
}
