using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules;

public partial class Modulo : RoboExpertModule
{
    public override string Name => "Modulo";
    public override string Help => "73 modulo 8";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices([.. BigNumbers(999)])) + "modulo" + new GrammarBuilder(new Choices([.. Numbers])));

    public override void ProcessCommand(string command)
    {
        var m = CommandMatcher().Match(command);

        Speak((int.Parse(m.Groups[1].Value) % int.Parse(m.Groups[2].Value)).ToString());
        ExitSubmenu();
        Solve();
    }

    [GeneratedRegex(@"^(\d+) modulo (\d+)$")]
    private static partial Regex CommandMatcher();
}
