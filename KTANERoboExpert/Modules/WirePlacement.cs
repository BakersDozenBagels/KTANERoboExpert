using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class WirePlacement : RoboExpertModule
{
    public override string Name => "Wire Placement";
    public override string Help => "Blue (wire at C3)";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("black", "blue", "red", "white", "yellow")));

    public override void ProcessCommand(string command)
    {
        Speak(command switch
        {
            "black" => "alfa 1 red, 2 blue, 3 yellow, bravo 1 black, 2 white, charlie 3 blue, 4 red, delta 1 yellow, 2 yellow, 3 white",
            "blue" => "alfa 1 yellow, bravo 3 red, charlie 1 white, 2 blue, 3 yellow, 4 blue, delta 1 yellow, 2 white, 3 red, 4 black",
            "red" => "alfa 1 blue, 2 yellow, 4 black, bravo 1 red, 2 yellow, 4 white, charlie 1 blue, 4 red, delta 2 yellow, 4 white",
            "white" => "alfa 1 white, 2 yellow, 4 yellow, bravo 2 red, 3 white, 4 yellow, charlie 1 red, 4 blue, delta 2 black, 3 blue",
            "yellow" => "alfa 3 yellow, 4 yellow, bravo 1 blue, 2 white, 3 red, 4 black, charlie 1 white, 2 red, delta 1 yellow, 4 blue",
            _ => throw new UnreachableException()
        });
        ExitSubmenu();
        Solve();
    }

    public override void Select() => Speak("Go on wire placement charlie 3");
}
