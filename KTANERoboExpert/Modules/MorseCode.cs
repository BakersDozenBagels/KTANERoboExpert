using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class MorseCode : RoboExpertModule
{
    public override string Name => "Morse Code";
    public override string Help => "go dot dash done | repeat | reset";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices("go" + new GrammarBuilder(new Choices("dot", "dash"), 1, 4) + "done", "repeat", "reset"));
    private string _soFar = string.Empty;

    public override void ProcessCommand(string command)
    {
        if (command == "reset")
        {
            Speak("Reset");
            Reset();
            return;
        }

        if (command == "repeat")
            _soFar += "X ";
        else
        {
            if (_soFar.IndexOf('X') is var x && x is not -1)
                _soFar = _soFar[(x + 2)..];

            _soFar +=
                command[3..^5]
                .Split(' ')
                .Select(c => c switch { "dot" => ".", "dash" => "-", _ => throw new UnreachableException() })
                .Conjoin(string.Empty)
                + ' ';
        }

        int val = Match(_soFar);
        while (val is 0)
        {
            _soFar = _soFar[(_soFar.IndexOf(' ') + 1)..];
            val = Match(_soFar);
        }
        Speak(val < 100 ? "Noted" : val.ToString());
        if (val > 100)
        {
            Reset();
            ExitSubmenu();
            Solve();
        }
    }

    private static int Match(string str)
    {
        str = str.TrimEnd();
        List<string> possible;
        if (str.Contains('X'))
        {
            var parts = str.Split(' ');
            var x = Array.IndexOf(parts, "X");
            possible = [.. _words.Keys];
            if (x != 0)
            {
                var end = parts[..x].Conjoin();
                possible.RemoveAll(p => !p.EndsWith(end));
            }
            if (x != parts.Length - 1)
            {
                var start = parts[(x + 1)..].Conjoin();
                possible.RemoveAll(p => !p.StartsWith(start));
            }
        }
        else
            possible = _words.Keys.Where(p => p.Contains(str)).ToList();

        return possible.Count == 1 ? _words[possible[0]] : possible.Count;
    }

    public override void Cancel() => Reset();
    public override void Reset() => _soFar = string.Empty;

    private static readonly Dictionary<string, int> _words = new()
    {
        ["... .... . .-.. .-.."] = 505,
        [".... .- .-.. .-.. ..."] = 515,
        ["... .-.. .. -.-. -.-"] = 522,
        ["- .-. .. -.-. -.-"] = 532,
        ["-... --- -..- . ..."] = 535,
        [".-.. . .- -.- ..."] = 542,
        ["... - .-. --- -... ."] = 545,
        ["-... .. ... - .-. ---"] = 552,
        ["..-. .-.. .. -.-. -.-"] = 555,
        ["-... --- -- -... ..."] = 565,
        ["-... .-. . .- -.-"] = 572,
        ["-... .-. .. -.-. -.-"] = 575,
        ["... - . .- -.-"] = 582,
        ["... - .. -. --."] = 592,
        ["...- . -.-. - --- .-."] = 595,
        ["-... . .- - ..."] = 600,
    };
}
