using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class ArtAppreciation : RoboExpertModule
{
    public override string Name => "Art Appreciation";
    public override string Help => "";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder("unused"));

    public override void ProcessCommand(string command) => throw new UnreachableException();

    public override void Select()
    {
        Speak("Appreciate the art.");
        ExitSubmenu();
        Solve();
    }
}
