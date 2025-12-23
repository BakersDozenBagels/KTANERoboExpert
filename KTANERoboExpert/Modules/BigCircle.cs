using KTANERoboExpert.Uncertain;
using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class BigCircle : RoboExpertModule
{
    public override string Name => "Big Circle";
    public override string Help => "clockwise | counterclockwise";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("clockwise", "counterclockwise", "widdershins")));

    public override void ProcessCommand(string command)
    {
        var x = Edgework.Indicators.Map(i => i.Sum(n => n.Label switch { "BOB" or "CAR" or "CLR" => 1, "FRK" or "FRQ" or "MSA" or "NSA" => 2, "SIG" or "SND" or "TRN" => 3, _ => 0 } * (n.Lit ? 1 : -1))).Into() +
            3 * Edgework.Solves +
            Edgework.Batteries.Map(x => x % 2 == 0 ? -4 : 4).Into() +
            Edgework.PortPlates.Map(p => p.Sum(l => l switch { { Parallel: true, Serial: false } => 5, { Parallel: true, Serial: true } => -4, { DVID: true, StereoRCA: false } => -5, { DVID: true, StereoRCA: true } => 4, _ => 0 })).Into() +
            Edgework.Indicators.Map(i => i.Sum(n => n.Label is "BOB" or "CAR" or "CLR" or "FRK" or "FRQ" or "MSA" or "NSA" or "SIG" or "SND" or "TRN" or "IND" ? 0 : 6)).Into() +
            // Modded ports omitted
            Edgework.TwoFactorCount * UncertainInt.InRange(0, 9, (_, __) => throw new NotImplementedException());

        var z = x.Map(y => Math.Abs(y) % 10).Map(y => y > 5 ? 10 - y : y).FlatMap(y => Edgework.SerialNumberCharacters()[y]);

        var wid = command is not "clockwise";

        if (!z.IsCertain)
        {
            z.Fill(() => ProcessCommand(command), ExitSubmenu);
            return;
        }

        var sol = _table[z.Value switch
        {
            >= '0' and <= '9' => z.Value - '0',
            >= 'A' and <= 'Z' => z.Value - 'A' + 10,
            _ => throw new UnreachableException()
        } / 3];

        if (wid)
        {
            sol = [.. sol];
            sol.Reverse();
        }

        Speak(sol.Conjoin());
        ExitSubmenu();
        Solve();
    }

    private static readonly string[][] _table = [
        ["Red", "Yellow", "Blue"],
        ["Orange", "Green", "Magenta"],
        ["Blue", "Black", "Red"],
        ["Magenta", "White", "Orange"],
        ["Orange", "Blue", "Black"],
        ["Green", "Red", "White"],
        ["Magenta", "Yellow", "Black"],
        ["Red", "Orange", "Yellow"],
        ["Yellow", "Green", "Blue"],
        ["Blue", "Magenta", "Red"],
        ["Black", "White", "Green"],
        ["White", "Yellow", "Blue"],
    ];
}
