using System.Speech.Recognition;
using System.Text.RegularExpressions;

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

        switch (colors.Length)
        {
            case 3:
                if (!colors.Contains("red"))
                    Speak("Cut second");
                else if (colors[2] == "white")
                    Speak("Cut third");
                else if (colors.Count(s => s == "blue") > 1)
                    Speak("Cut " + ord[Array.LastIndexOf(colors, "blue")]);
                else
                    Speak("Cut third");
                ExitSubmenu();
                break;
            case 4:
                if (colors.Count(s => s == "red") > 1 && Edgework.SerialNumber == null)
                {
                    CheckSN(command);
                    break;
                }
                if (colors.Count(s => s == "red") > 1 && (Edgework.SerialNumber![5] - '0') % 2 == 1)
                    Speak("Cut " + ord[Array.LastIndexOf(colors, "red")]);
                else if ((colors[3] == "yellow" && !colors.Contains("red")) || colors.Count(s => s == "blue") == 1)
                    Speak("Cut first");
                else if (colors.Count(s => s == "yellow") > 1)
                    Speak("Cut fourth");
                else
                    Speak("Cut second");
                ExitSubmenu();
                break;
            case 5:
                if (colors[4] == "black" && Edgework.SerialNumber == null)
                {
                    CheckSN(command);
                    break;
                }
                if (colors[4] == "black" && (Edgework.SerialNumber![5] - '0') % 2 == 1)
                    Speak("Cut fourth");
                else if (colors.Count(s => s == "red") == 1 && colors.Count(s => s == "yellow") > 1)
                    Speak("Cut first");
                else if (!colors.Contains("black"))
                    Speak("Cut second");
                else
                    Speak("Cut first");
                ExitSubmenu();
                break;
            case 6:
                if (!colors.Contains("yellow") && Edgework.SerialNumber == null)
                {
                    CheckSN(command);
                    break;
                }
                if (!colors.Contains("yellow") && (Edgework.SerialNumber![5] - '0') % 2 == 1)
                    Speak("Cut third");
                else if (colors.Count(s => s == "yellow") == 1 && colors.Count(s => s == "white") > 1)
                    Speak("Cut fourth");
                else if (!colors.Contains("red"))
                    Speak("Cut sixth");
                else
                    Speak("Cut fourth");
                ExitSubmenu();
                break;
            default:
                SpeakSync("Pardon?");
                break;
        }
    }

    private void CheckSN(string command)
    {
        RequestEdgeworkFill(EdgeworkType.SerialNumber, () => ProcessCommand(command), () => _checkingEdgework = false);
        _checkingEdgework = true;
    }

    public override void Reset() => _checkingEdgework = false;

    [GeneratedRegex("(red|yellow|black|white|blue)(?: wire)?", RegexOptions.Compiled)]
    private static partial Regex CommandMatcher();
}
