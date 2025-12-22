using System.Diagnostics;
using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules;

public class TextField : RoboExpertModule
{
    public override string Name => "Text Field";
    public override string Help => "alfa";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices([.. NATO.Take(6)])));

    public override void ProcessCommand(string command) =>
        (command[0] switch
        {
            'a' => UncertainCondition<Table>.Of(Edgework.HasIndicator("CLR", lit: true), Table._1459)
                | (Edgework.Batteries > 2, Table._BBFF)
                | (Edgework.Batteries == 1, Table._7F67)
                | (Edgework.HasIndicator("FRK", lit: true), Table._DC52)
                | Table._A0C1,
            'b' => UncertainCondition<Table>.Of(Edgework.Batteries == 0, Table._965A)
                | (Edgework.SerialNumberDigits()[^1].Into().IsOdd(), Table._1459)
                | (!Edgework.Ports.Contains(Edgework.PortType.Serial), Table._DC52)
                | (Edgework.HasIndicator("TRN", lit: true), Table._A0C1)
                | Table._7F67,
            'c' => UncertainCondition<Table>.Of(Edgework.Ports.Contains(Edgework.PortType.DVID), Table._AA12)
                | (Edgework.Batteries == 2, Table._FB01)
                | (Edgework.SerialNumberVowels().Count == 0, Table._DC52)
                | (Edgework.HasIndicator("CAR", lit: true), Table._1459)
                | Table._7F67,
            'd' => UncertainCondition<Table>.Of(Edgework.Ports.Contains(Edgework.PortType.Parallel), Table._FB01)
                | (Edgework.Batteries < 2, Table._AA12)
                | (Edgework.HasIndicator("SIG", lit: true), Table._BBFF)
                | (!Edgework.Ports.Contains(Edgework.PortType.PS2), Table._965A)
                | Table._1459,
            'e' => UncertainCondition<Table>.Of(Edgework.Batteries < 3, Table._7F67)
                | (!Edgework.Ports.Contains(Edgework.PortType.StereoRCA), Table._AA12)
                | (Edgework.HasIndicator("BOB", lit: true), Table._A0C1)
                | (Edgework.Ports.Contains(Edgework.PortType.RJ45), Table._BBFF)
                | Table._DC52,
            'f' => UncertainCondition<Table>.Of(!Edgework.Ports.Contains(Edgework.PortType.Serial), Table._DC52)
                | (Edgework.SerialNumberVowels().Count > 0, Table._A0C1)
                | (Edgework.HasIndicator("IND", lit: true), Table._1459)
                | (Edgework.SerialNumberDigits()[^1].Into().IsEven(), Table._FB01)
                | Table._AA12,
            _ => throw new UnreachableException()
        })
        .Map(v => _table[(int)v].AllIndicesOf(command[0]).Select(ToIndex).Conjoin())
        .Do(u => u.Fill(() => ProcessCommand(command), ExitSubmenu),
            v =>
            {
                Speak(v);
                ExitSubmenu();
                Solve();
            });

    private static string ToIndex(int ix) => NATO.ElementAt(ix % 4) + " " + (ix / 4 + 1);

    private static readonly char[][] _table =
    [
        // Table FB01
        [
            'd', 'c', 'f', 'a',
            'b', 'e', 'f', 'f',
            'b', 'b', 'b', 'c',
        ],
        // Table 965A
        [
            'c', 'b', 'e', 'f',
            'e', 'b', 'f', 'e',
            'd', 'c', 'a', 'a',
        ],
        // Table 1459
        [
            'b', 'a', 'b', 'b',
            'c', 'd', 'f', 'd',
            'd', 'f', 'c', 'e',
        ],
        // Table BBFF
        [
            'd', 'a', 'b', 'f',
            'd', 'f', 'b', 'e',
            'c', 'e', 'b', 'a',
        ],
        // Table DC52
        [
            'c', 'b', 'd', 'e',
            'a', 'f', 'd', 'c',
            'b', 'e', 'b', 'd',
        ],
        // Table 7F67
        [
            'a', 'd', 'c', 'b',
            'a', 'c', 'b', 'c',
            'a', 'e', 'f', 'a',
        ],
        // Table A0C1
        [
            'e', 'c', 'f', 'a',
            'c', 'f', 'b', 'd',
            'f', 'f', 'b', 'c',
        ],
        // Table AA12
        [
            'b', 'e', 'a', 'b',
            'e', 'd', 'f', 'a',
            'b', 'c', 'e', 'c',
        ],
    ];

    private enum Table
    {
        _FB01 = 0,
        _965A = 1,
        _1459 = 2,
        _BBFF = 3,
        _DC52 = 4,
        _7F67 = 5,
        _A0C1 = 6,
        _AA12 = 7,
    }
}
