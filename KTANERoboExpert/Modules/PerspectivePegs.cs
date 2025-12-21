using System.Diagnostics;
using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules;

public class PerspectivePegs : RoboExpertModule
{
    public override string Name => "Perspective Pegs";
    public override string Help => "red yellow green blue purple";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("red", "yellow", "green", "blue", "purple"), 5, 5));

    public override void ProcessCommand(string command)
    {
        var sequence = command.Split(' ').Select(s => s[0].ToString()).Conjoin("");

        var check = _permutations[Edgework.Batteries.Value! switch
        {
            1 or 2 => 0,
            3 or 4 => 1,
            _ => 2
        }];

        foreach (var c in check)
            sequence = Replace(sequence, c.prime, c.alternate);

        Speak(sequence.Take(3).Select(c => _colors.First(s => s[0] == c)).Conjoin());
        ExitSubmenu();
        Solve();
    }

    private static string Replace(string sequence, string prime, string alternate)
    {
        if (sequence.IndexOf(prime) is var i and not -1)
            sequence = i switch
            {
                0 => alternate + sequence[3..],
                1 => sequence[0] + alternate + sequence[4],
                2 => sequence[..2] + alternate,
                _ => throw new UnreachableException()
            };

        if (sequence.LastIndexOf(Reverse(prime)) is var j and not -1)
            sequence = j switch
            {
                0 => Reverse(alternate) + sequence[3..],
                1 => sequence[0] + Reverse(alternate) + sequence[4],
                2 => sequence[..2] + Reverse(alternate),
                _ => throw new UnreachableException()
            };

        return sequence;
    }

    private static string Reverse(string s) => new([.. s.Reverse()]);

    private static readonly string[] _colors = ["red", "yellow", "green", "blue", "purple"];

    private static readonly (string prime, string alternate)[][] _permutations = [
        // 1-2 Batteries
        [
            ("ryy", "bpy"),
            ("ypg", "pbr"),
            ("rgp", "bgr"),
            ("ybg", "byy"),
            ("ppr", "ryp"),
            ("bgb", "pyg"),
            ("ygb", "gpy"),
            ("pgg", "gyr"),
        ],
        // 3-4 Batteries
        [
            ("bpb", "ybg"),
            ("yyp", "brp"),
            ("grb", "ypb"),
            ("rpy", "gbg"),
            ("ygg", "pbr"),
            ("gpb", "ygy"),
            ("prp", "bbg"),
            ("ryr", "rpb"),
        ],
        // 0,5+ Batteries
        [
            ("pyb", "rgb"),
            ("yrp", "ryr"),
            ("gyr", "gbp"),
            ("byg", "pgr"),
            ("rpy", "gyb"),
            ("ppg", "pbr"),
            ("ryy", "bbr"),
            ("ygp", "pyy"),
        ],
    ];
    private static readonly string[] _keys = ["red", "green", "purple", "red", "yellow", "blue", "purple", "green", "blue", "yellow"];

    public override void Select()
    {
        if (!Edgework.SerialNumber.IsCertain)
        {
            Edgework.SerialNumber.Fill(Select, ExitSubmenu);
            return;
        }

        if (!Edgework.Batteries.IsCertain)
        {
            Edgework.Batteries.Fill(Select, ExitSubmenu);
            return;
        }

        var key = _keys[Edgework.SerialNumberLetters().Value!.ToArray() switch
        {
            [var a, var b] => Math.Abs(b - a),
            [var a, var b, _] => Math.Abs(b - a),
            [var a, var b, var c, var d] => Math.Abs(b - a) + Math.Abs(d - c),
            _ => throw new UnreachableException()
        } % 10];

        Speak("Key color " + key);
    }
}
