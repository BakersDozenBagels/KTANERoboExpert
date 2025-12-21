using System.Diagnostics;
using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules.Vanilla;

public class SimonSays : RoboExpertModule
{
    public override string Name => "Simon Says";
    public override string Help => "Red Red Yellow Green Done";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("red", "yellow", "green", "blue"), 1, 5) + "done");

    public override void ProcessCommand(string command)
    {
        if (!Edgework.SerialNumber.IsCertain)
        {
            Edgework.SerialNumber.Fill(() => ProcessCommand(command), ExitSubmenu);
            return;
        }

        string[] names = ["red", "blue", "green", "yellow"];
        int[] table =
            (Edgework.SerialNumberVowels().Value!.Any(), Edgework.Strikes) switch
            {
                (true, 0) => [1, 0, 3, 2],
                (true, 1) => [3, 2, 1, 0],
                (true, _) => [2, 0, 3, 1],
                (false, 0) => [1, 3, 2, 0],
                (false, 1) => [0, 1, 3, 2],
                (false, _) => [3, 2, 1, 0],
            };

        var colors = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Speak(colors.SkipLast(1).Select(p => p switch
        {
            "red" => names[table[0]],
            "blue" => names[table[1]],
            "green" => names[table[2]],
            "yellow" => names[table[3]],
            _ => throw new UnreachableException($"Unexpected color {p}")
        }).Conjoin());
        if (colors.Length == 5)
        {
            ExitSubmenu();
            Solve();
        }
    }

    public override void Select()
    {
        base.Select();
        Edgework.SerialNumber.Fill(() => { }, ExitSubmenu);
    }

    public override void Cancel() => Load(Solve);
}
