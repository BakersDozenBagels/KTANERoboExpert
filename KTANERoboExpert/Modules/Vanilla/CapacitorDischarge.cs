using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules.Vanilla;

public class CapacitorDischarge : RoboExpertModule
{
    public override string Name => "Capacitor Discharge";
    public override string Help => "";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder("unused"));

    public override void ProcessCommand(string command) => throw new UnreachableException();

    public override void Select()
    {
        Speak("Hold the lever.");
        ExitSubmenu();
    }
}
