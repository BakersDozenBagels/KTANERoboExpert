using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class Egg : RoboExpertModule
{
    public override string Name => "egg";
    public override string Help => "";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder("unused"));

    public override void ProcessCommand(string command) => throw new UnreachableException();

    public override void Select()
    {
        if (Edgework.SerialNumber.IsCertain)
            Speak("egg on " + Edgework.SerialNumberDigits().Last());
        else
            Speak("egg on the last digit of the serial number");

        ExitSubmenu();
        Solve();
    }
}
