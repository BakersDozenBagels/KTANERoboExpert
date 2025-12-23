using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules;

public class Code : RoboExpertModule
{
    public override string Name => "Code";
    public override string Help => "3 8 7 5";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(BigNumbers(9)), 4, 4));

    public override void ProcessCommand(string command)
    {
        var n = int.Parse(new([.. command.Split(' ').Select(s => s[0])]));
        var sol = (UncertainCondition<int>.Of(Edgework.SerialNumberDigits()[0].Into() == Edgework.SerialNumberDigits()[1].Into() & Edgework.Batteries == 0, 1)
            | (Edgework.HasIndicator("CLR"), 8)
            | (Edgework.SerialNumber.Map(s => s.Intersect(['X', 'Y', 'Z']).Any()).Into(), 20)
            | (Edgework.Ports.Count >= 5, 30)
            | (Edgework.Batteries == 0, 42)
            | (Edgework.Indicators.Count(x => x.Lit) > Edgework.Indicators.Count(x => !x.Lit), 69)
            | 3).Map(x => n / x);

        if (!sol.IsCertain)
        {
            sol.Fill(() => ProcessCommand(command), ExitSubmenu);
            return;
        }

        Speak(sol.Value.ToString());
        ExitSubmenu();
        return;
    }
}
