using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class Knob : RoboExpertModule
{
    public override string Name => "Knob";
    public override string Help => "columns 3 6 done | columns done";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new("columns" + new GrammarBuilder(new Choices("3 6", "3 5", "2 3 6", "5", "1 3 5", "1 3"), 0, 1) + "done");

    public override void ProcessCommand(string command)
    {
        if (command == "columns done")
            Speak("Down");
        else
            Speak(command[8..^5] switch
            {
                "3 6" or "3 5" => "Up",
                "2 3 6" => "Down",
                "5" => "Left",
                "1 3 5" or "1 3" => "Right",
                _ => throw new UnreachableException()
            });
        ExitSubmenu();
    }
}
