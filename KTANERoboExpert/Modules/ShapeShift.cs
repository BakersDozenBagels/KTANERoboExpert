using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules;

public class ShapeShift : RoboExpertModule
{
    public override string Name => "Shape Shift";
    public override string Help => "ticket point";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(_shapes), 2, 2));

    public override void ProcessCommand(string command)
    {
        var p = command.Split(' ');
        var i = Array.IndexOf(_shapes, p[0]) * 4 + Array.IndexOf(_shapes, p[1]);

        HashSet<int> done = [i];
        while (true)
        {
            var j = _table[i];
            var k = j.cond().Map(b => b ? j.t : j.f);
            if (!k.IsCertain)
            {
                k.Fill(() => ProcessCommand(command), ExitSubmenu);
                return;
            }
            if (!done.Add(k.Value))
            {
                Speak(_shapes[k.Value / 4] + " " + _shapes[k.Value % 4]);
                ExitSubmenu();
                Solve();
                return;
            }
            i = k.Value;
        }
    }

    private static readonly string[] _shapes = ["square", "round", "ticket", "point"];

    private static readonly (Func<UncertainBool> cond, int t, int f)[] _table =
    [
        (() => Edgework.SerialNumberDigits()[^1].Into().IsOdd(), 12, 13),
        (() => Edgework.Ports.Contains(Edgework.PortType.DVID), 7, 11),
        (() => Edgework.HasIndicator("BOB", lit: false), 9, 15),
        (() => Edgework.HasIndicator("MSA", lit: true), 11, 12),
        (() => Edgework.HasIndicator("SND", lit: true), 5, 6),
        (() => Edgework.SerialNumberVowels().Count > 0, 10, 1),
        (() => Edgework.AABatteries >= 2, 8, 2),
        (() => Edgework.HasIndicator("SIG", lit: true), 10, 0),
        (() => Edgework.HasIndicator("FRQ", lit: false), 1, 15),
        (() => Edgework.Ports.Contains(Edgework.PortType.StereoRCA), 3, 4),
        (() => Edgework.Batteries >= 3, 8, 2),
        (() => Edgework.Ports.Contains(Edgework.PortType.PS2), 14, 13),
        (() => Edgework.HasIndicator("CAR", lit: false), 9, 6),
        (() => Edgework.Ports.Contains(Edgework.PortType.Parallel), 3, 4),
        (() => Edgework.Ports.Contains(Edgework.PortType.RJ45), 7, 5),
        (() => Edgework.HasIndicator("IND", lit: true), 14, 0),
    ];
}
