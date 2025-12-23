using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules;

public partial class SpinningButtons : RoboExpertModule
{
    public override string Name => "Spinning Buttons";
    public override string Help => "Red Theta Orange V Gray QV Blue HD (Theta V QV QH H Triangle)";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new GrammarBuilder(new Choices("red", "purple", "orange", "grey", "green", "blue")) + new Choices("theta", "v", "q v", "q h", "h", "triangle"), 4, 4));

    public override void ProcessCommand(string command)
    {
        var m = CommandMatcher().Match(command);
        Speak(m.Groups[1].Captures.Zip(m.Groups[2].Captures).Select(t => (t.First.Value, t.Second.Value)).OrderBy(t => Value(t.Item1, t.Item2)).Select(t => t.Item1 + " " + t.Item2).Conjoin(", "));
        ExitSubmenu();
        Solve();
    }

    private static int Value(string color, string label) =>
        ((string[])["red", "purple", "orange", "grey", "green", "blue"]).IndexOf(color) +
        ((string[])["theta", "v", "q v", "q h", "h", "triangle"]).IndexOf(label);

    [GeneratedRegex("^(?:(red|purple|orange|grey|green|blue) (theta|v|q v|q h|h|triangle) ?){4}$")]
    private static partial Regex CommandMatcher();
}
