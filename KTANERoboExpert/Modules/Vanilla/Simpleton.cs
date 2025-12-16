using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules.Vanilla;

public class Simpleton : RoboExpertModule
{
    public override string Name => "Simpleton";
    public override string Help => "";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder("unused"));

    public override void ProcessCommand(string command) => throw new UnreachableException();

    public override void Select()
    {
        Speak("Push the button.");
        ExitSubmenu();
        Solve();
    }
}
