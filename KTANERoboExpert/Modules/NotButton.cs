using KTANERoboExpert.Uncertain;
using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class NotButton : RoboExpertModule
{
    public override string Name => "Not Button";
    public override string Help => "red abort";
    private Grammar? _grammar, _subGrammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(_colors)) + new Choices(_labels));
    private Grammar SubGrammar => _subGrammar ??= new Grammar(new GrammarBuilder(new Choices(_holdColors), 1, 2));

    private bool _holding = false;

    public override void ProcessCommand(string command)
    {
        if (_holding)
        {
            string when = command switch
            {
                "white" or "white white" => "whenever",
                "red" or "red red" => "on 1",
                "yellow" or "yellow yellow" => "on any 4",
                "green" or "green green" => "on add to 7",
                "blue" or "blue blue" => "when 10s is 0 2 3 or 5",
                "red yellow" or "yellow red" => Edgework.SerialNumberDigits()[^1].Match(x => "on any " + x, "on any last digit of serial number"),
                "red green" or "green red" => "on 0 2 3 5 or 7",
                "red blue" or "blue red" => "when seconds digit matches leftmost timer digit",
                "white red" or "red white" => "on any 9",
                "white yellow" or "yellow white" => Edgework.Batteries.Match(x => "on any " + x, "on any battery count"),
                "white green" or "green white" => "on 0 11 22 33 44 or 55",
                "white blue" or "blue white" => "on not 7",
                "yellow blue" or "blue yellow" => "on any 6",
                "yellow green" or "green yellow" => "on difference of 4",
                "green blue" or "blue green" => "when 10s digit is not 2",
                _ => throw new UnreachableException()
            };

            Speak("release " + when);
            ExitSubmenu();
            ExitSubmenu();
            _holding = false;
            Solve();
            return;
        }

        var btn = command.Split(' ');
        var color = btn[0];
        var label = btn[1];

        var sol = _table[Array.IndexOf(_colors, color)][Array.IndexOf(_labels, label)];
        switch (sol)
        {
            case Solution.Press:
                Press();
                return;
            case Solution.Mash:
                Mash(color, label);
                return;
            case Solution.Hold:
                Hold();
                return;
            default:
                throw new UnreachableException();
        }
    }

    private static readonly string[] _colors = ["red", "orange", "yellow", "green", "cyan", "blue", "purple", "pink", "white", "black"];
    private static readonly string[] _holdColors = ["white", "red", "yellow", "green", "blue"];
    private static readonly string[] _labels = ["Press", "Hold", "Detonate", "Tap", "Push", "Abort", "Button", "Click", "Mash"];
    private static readonly Solution[][] _table = [
        // Red
        [Solution.Press, Solution.Mash, Solution.Hold, Solution.Press, Solution.Hold, Solution.Hold, Solution.Press, Solution.Mash, Solution.Press],
        //Orange
        [Solution.Mash, Solution.Press, Solution.Press, Solution.Hold, Solution.Mash, Solution.Mash, Solution.Mash, Solution.Mash, Solution.Mash],
        // Yellow
        [Solution.Hold, Solution.Press, Solution.Mash, Solution.Mash, Solution.Press, Solution.Hold, Solution.Press, Solution.Press, Solution.Hold],
        // Green
        [Solution.Press, Solution.Hold, Solution.Press, Solution.Mash, Solution.Mash, Solution.Hold, Solution.Press, Solution.Press, Solution.Press],
        // Cyan
        [Solution.Hold, Solution.Mash, Solution.Mash, Solution.Press, Solution.Hold, Solution.Press, Solution.Hold, Solution.Press, Solution.Mash],
        // Blue
        [Solution.Press, Solution.Hold, Solution.Press, Solution.Mash, Solution.Press, Solution.Hold, Solution.Mash, Solution.Hold, Solution.Press],
        // Purple
        [Solution.Mash, Solution.Hold, Solution.Hold, Solution.Press, Solution.Mash, Solution.Mash, Solution.Hold, Solution.Mash, Solution.Hold],
        // Pink
        [Solution.Mash, Solution.Press, Solution.Hold, Solution.Press, Solution.Press, Solution.Press, Solution.Mash, Solution.Hold, Solution.Mash],
        // White
        [Solution.Press, Solution.Mash, Solution.Press, Solution.Hold, Solution.Mash, Solution.Press, Solution.Press, Solution.Press, Solution.Hold],
        // Black
        [Solution.Hold, Solution.Hold, Solution.Mash, Solution.Mash, Solution.Press, Solution.Mash, Solution.Hold, Solution.Mash, Solution.Mash],
    ];

    private enum Solution
    {
        Press,
        Hold,
        Mash
    }

    private void Press()
    {
        Speak("Tap");
        ExitSubmenu();
        Solve();
    }

    private void Hold()
    {
        SpeakSSML("Hold<break time=\"500ms\"/>Strip color?");
        EnterSubmenu(SubGrammar);
        _holding = true;
    }

    private void Mash(string color, string label)
    {
        UncertainInt
            a = Edgework.Batteries,
            b = Edgework.PortTypes.Count,
            c = Edgework.SolvableModuleCount,
            d = Edgework.Indicators.Count,
            e = Edgework.SerialNumberDigits()[^1].Into(0, 9),
            f = Edgework.SerialNumberLetters()[1].Map(c => c - 'A' + 1).Into(1, 26),
            g = label.Length;

        UncertainInt amount = color switch
        {
            "red" => a + 2 * b - d,
            "orange" => 2 * b + 1 - g,
            "yellow" => 2 * a + d - c,
            "green" => d + 2 * f - b,
            "cyan" => e + f + g - b,
            "blue" => 2 * c + d - 1,
            "purple" => 2 * (f - a) + d,
            "pink" => 3 * g - (a + 3),
            "white" => (f + a * c) * (e + d),
            "black" => a * b + c * d - g * (e - f),
            _ => throw new UnreachableException()
        };

        if (!amount.IsCertain)
            amount.Fill(() => ProcessCommand(color + " " + label), ExitSubmenu);
        else
        {
            var v = amount.Value;
            while (v < 10)
                v += 7;
            while (v > 99)
                v -= 7;

            Speak("Mash " + v);
            ExitSubmenu();
            Solve();
        }
    }

    public override void Cancel()
    {
        if (_holding)
        {
            ExitSubmenu();
            _holding = false;
        }
    }

    public override void Reset() => _holding = false;

    public override void Select()
    {
        _holding = false;
        base.Select();
    }
}
