using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules.Needy;

public class RefillThatBeer : RoboExpertModule
{
    public override string Name => "Refill That Beer";
    public override string Help => "";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder("unused"));

    public override void ProcessCommand(string command) => throw new UnreachableException();

    public override void Select()
    {
        Speak("Refill that beer!");
        ExitSubmenu();
    }
}
