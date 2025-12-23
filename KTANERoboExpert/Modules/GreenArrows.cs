using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class GreenArrows : RoboExpertModule
{
    public override string Name => "Green Arrows";
    public override string Help => "47 -> 3 -> ...";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices(BigNumbers(99)));
    private int _stagesDone;

    public override void ProcessCommand(string command)
    {
        int i = int.Parse(command), t = i / 10, o = i % 10, x = (t + 9) % 10 + 10 * ((10 - o) % 10);

        Speak(_directions[x]);
        _stagesDone++;
        if (_stagesDone == 7)
        {
            _stagesDone = 0;
            ExitSubmenu();
            Solve();
        }
    }

    public override void Select() => Speak("Go on Green Arrows stage " + (_stagesDone + 1));
    public override void Reset() => _stagesDone = 0;

    private static readonly string[] _directions = [
        "Up", "Right", "Left", "Right", "Up", "Right", "Left", "Right", "Up", "Down",
        "Left", "Right", "Up", "Down", "Left", "Down", "Up", "Down", "Left", "Right",
        "Down", "Up", "Right", "Left", "Right", "Down", "Right", "Left", "Down", "Up",
        "Up", "Down", "Up", "Down", "Up", "Right", "Left", "Right", "Up", "Down",
        "Left", "Right", "Left", "Right", "Left", "Down", "Down", "Up", "Left", "Right",
        "Down", "Up", "Down", "Up", "Down", "Up", "Left", "Down", "Down", "Up",
        "Up", "Down", "Right", "Up", "Right", "Down", "Up", "Left", "Up", "Down",
        "Left", "Right", "Up", "Right", "Up", "Right", "Right", "Up", "Left", "Right",
        "Down", "Up", "Down", "Up", "Down", "Up", "Up", "Right", "Down", "Up",
        "Up", "Down", "Right", "Left", "Down", "Left", "Right", "Up", "Down", "Left",
    ];
}
