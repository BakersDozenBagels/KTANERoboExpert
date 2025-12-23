using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;
using KTANERoboExpert.Modules.Vanilla;

namespace KTANERoboExpert.Modules;

public class RoundKeypad : RoboExpertModule
{
    public override string Name => "Round Keypad";
    public override string Help => "lollipop then lambda then ...";
    private Grammar? _grammar;

    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new GrammarBuilder(new Choices([.. Keypad.SymbolNames.Keys])) + "then", 7, 7) + new Choices([.. Keypad.SymbolNames.Keys]));

    public override void ProcessCommand(string command)
    {
        var symbols = command.Split(" then ").Select(x => Keypad.SymbolNames[x]).ToArray();
        var col = _table.MaxBy(s => symbols.Count(s.Contains));

        Speak(symbols.Select((s, i) => (s, i)).Where(t => !col!.Contains(t.s)).Select(t => _ord[t.i]).Conjoin(", "));
        ExitSubmenu();
        Solve();
    }

    private static readonly string[] _ord = ["first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth"];

    private static readonly HashSet<int>[] _table = [
        [15, 7, 23, 24, 19, 25, 26],
        [19, 18, 17, 20, 16, 21, 22],
        [15, 16, 17, 4, 13, 10, 18],
        [11, 12, 8, 13, 14, 2, 9],
        [7, 0, 6, 8, 9, 5, 10],
        [0, 1, 2, 3, 4, 5, 6],
    ];
}
