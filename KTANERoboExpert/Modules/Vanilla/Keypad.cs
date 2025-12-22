using System.Speech.Recognition;

namespace KTANERoboExpert.Modules.Vanilla;

public partial class Keypad : RoboExpertModule
{
    public override string Name => "Keypad";
    public override string Help => "backwards c then euro then train tracks then controller";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(SymbolNames.Keys.ToArray()).ToGrammarBuilder() + "then", 3, 3) + new Choices(SymbolNames.Keys.ToArray()));

    public override void ProcessCommand(string command)
    {
        var symbols = command.Split(" then ").Select(x => SymbolNames[x]).ToArray();
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
}
