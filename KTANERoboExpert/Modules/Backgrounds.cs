using KTANERoboExpert.Uncertain;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class Backgrounds : RoboExpertModule
{
    public override string Name => "Backgrounds";
    public override string Help => "white on orange";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("red", "orange", "yellow", "green", "blue", "purple", "white", "gray", "black")) + "on" + new GrammarBuilder(new Choices("red", "orange", "yellow", "green", "blue", "purple", "white", "gray", "black")));

    private static readonly string[] _colors = ["red", "orange", "yellow", "green", "blue", "purple", "white", "gray", "black"];
    private static readonly int[] _a = [0, 3, 2, 3, 1, 5, 4, 1, 2, 4];
    private static readonly int[][] _table = [[3, 2, 9, 1, 7, 4], [7, 9, 8, 8, 2, 3], [5, 1, 7, 4, 4, 6], [6, 4, 2, 6, 8, 5], [5, 1, 5, 3, 9, 9], [1, 2, 3, 6, 7, 8]];

    public override void ProcessCommand(string command)
    {
        if (command.Split(' ').Select(x => Array.IndexOf(_colors, x)).ToArray() is not [var button, _, var bg])
            return;

        var rowc = UncertainCondition<int>.Of(button == bg, 0)
            | ((button == 6 || button == 8) != (bg == 6 || bg == 8), 1)
            | (Edgework.DBatteries == 0, 2)
            | (Edgework.AABatteries == 0, 3)
            | ((button == 0 || button == 2 || button == 4) && (bg == 0 || bg == 2 || bg == 4), 4)
            | (button == 1 || button == 3 || button == 5, 5)
            | (Edgework.HasIndicator("SND", lit: false), 6)
            | (Edgework.PortPlates.Where(pl => pl.Serial).Count > 0, 7)
            | ((bg == 4 && button == 4) || ((bg == 2 || bg == 3) && button == 3) || ((bg == 0 || bg == 5) && button == 5), 8)
            | 9;

        if (!rowc.IsCertain)
        {
            rowc.Fill(() => ProcessCommand(command));
            return;
        }

        var row = rowc.Value;

        var colc = UncertainCondition<int>.Of(row != 1 && ((button == 6 || button == 8) != (bg == 6 || bg == 8)), 1)
            | (row != 2 & Edgework.DBatteries == 0, 4)
            | (row != 3 & Edgework.AABatteries == 0, 3)
            | (row != 4 && (button == 0 || button == 2 || button == 4) && (bg == 0 || bg == 2 || bg == 4), 5)
            | (row != 5 && (button == 1 || button == 3 || button == 5), 4)
            | (row != 6 & Edgework.HasIndicator("SND", lit: false), 1)
            | (row != 7 & Edgework.PortPlates.Where(pl => pl.Serial).Count > 0, 2)
            | (row != 8 && ((bg == 4 && button == 4) || ((bg == 2 || bg == 3) && button == 3) || ((bg == 0 || bg == 5) && button == 5)), 3)
            | 0;

        if (!colc.IsCertain)
        {
            colc.Fill(() => ProcessCommand(command));
            return;
        }

        Speak("Submit " + _table[_a[row]][colc.Value]);
        ExitSubmenu();
        Solve();
    }
}
