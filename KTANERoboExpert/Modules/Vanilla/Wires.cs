using System.Speech.Recognition;
using System.Text.RegularExpressions;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules;

public partial class Wires : RoboExpertModule
{
    public override string Name => "Wires";
    public override string Help => "red blue yellow black done";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("red", "yellow", "black", "white", "blue") + new GrammarBuilder("wire", 0, 1), 3, 6) + "done");

    private bool _checkingEdgework = false;

    public override void ProcessCommand(string command)
    {
        var colors = CommandMatcher().Matches(command).Select(m => m.Groups[1].Value).ToArray();

        string[] ord = ["first", "second", "third", "fourth", "fifth", "sixth"];
        if (!_checkingEdgework)
            SpeakSSML("<prosody rate=\"+40%\">" + colors.Select(c => c == "black" ? "k" : c[0].ToString()).Conjoin() + "</prosody>");
        _checkingEdgework = false;

        UncertainCondition<int> result;
        switch (colors.Length)
        {
            case 3:
                result = UncertainCondition<int>.Of(!colors.Contains("red"), 1) 
                    | (colors[2] == "white", 2)
                    | (colors.Count(s => s == "blue") > 1, Array.LastIndexOf(colors, "blue"))
                    | 2;
                break;
            case 4:
                result = UncertainCondition<int>.Of(colors.Count(s => s == "red") > 1 & Edgework.SerialNumber.Map(n => (n[5] - '0') % 2 == 1).Into(), Array.LastIndexOf(colors, "red"))
                    | ((colors[3] == "yellow" && !colors.Contains("red")) || colors.Count(s => s == "blue") == 1, 0)
                    | (colors.Count(s => s == "yellow") > 1, 3)
                    | 1;
                break;
            case 5:
                result = UncertainCondition<int>.Of(colors[4] == "black" & Edgework.SerialNumber.Map(n => (n[5] - '0') % 2 == 1).Into(), 3)
                    | (colors.Count(s => s == "red") == 1 && colors.Count(s => s == "yellow") > 1, 0)
                    | (!colors.Contains("black"), 1)
                    | 0;
                break;
            case 6:
                result = UncertainCondition<int>.Of(!colors.Contains("yellow") & Edgework.SerialNumber.Map(n => (n[5] - '0') % 2 == 1).Into(), 2)
                    | (colors.Count(s => s == "yellow") == 1 && colors.Count(s => s == "white") > 1, 3)
                    | (!colors.Contains("red"), 5)
                    | 3;
                break;
            default:
                SpeakSync("Pardon?");
                return;
        }

        if (result.IsCertain)
        {
            Speak("Cut " + ord[result.Value]);
            ExitSubmenu();
            Solve();
        }
        else
        {
            _checkingEdgework = true;
            result.Fill(() => ProcessCommand(command), () => _checkingEdgework = false);
        }
    }

    public override void Reset() => _checkingEdgework = false;

    [GeneratedRegex("(red|yellow|black|white|blue)(?: wire)?", RegexOptions.Compiled)]
    private static partial Regex CommandMatcher();
}
