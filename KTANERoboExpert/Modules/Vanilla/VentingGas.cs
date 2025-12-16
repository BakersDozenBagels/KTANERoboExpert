using System.Speech.Recognition;

namespace KTANERoboExpert.Modules.Vanilla;

public class VentingGas : RoboExpertModule
{
    public override string Name => "Venting Gas";
    public override string Help => "Vent gas | Detonate";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices("vent gas", "detonate"));

    public override void ProcessCommand(string command)
    {
        Speak(command == "detonate" ? "Press no" : "Press yes");
        ExitSubmenu();
    }
}
