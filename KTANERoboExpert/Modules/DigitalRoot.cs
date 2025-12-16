using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class DigitalRoot : RoboExpertModule
{
    public override string Name => "Digital Root";
    public override string Help => "1 2 3 6";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("0", "1", "2", "3", "4", "5", "6", "7", "8", "9"), 4, 4));

    public override void ProcessCommand(string command)
    {
        var nums = command.Split(' ').Select(int.Parse).ToArray();

        var x = (nums[0] + nums[1] + nums[2] - 1) % 9 + 1;
        Speak(x == nums[3] ? "Press yes" : "Press no");

        ExitSubmenu();
        Solve();
    }
}
