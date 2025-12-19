using KTANERoboExpert.Uncertain;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules.Vanilla;

public partial class Button : RoboExpertModule
{
    public override string Name => "Button";
    public override string Help => "red abort";
    private Grammar? _grammar, _subGrammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("red", "yellow", "white", "blue")) + new Choices("abort", "detonate", "hold", "press"));
    private Grammar SubGrammar => _subGrammar ??= new Grammar(new Choices("red", "yellow", "white", "blue"));

    private bool _holding = false;

    public override void ProcessCommand(string command)
    {
        if (_holding)
        {
            var dir = command switch
            {
                "red" or "white" => "1",
                "yellow" => "5",
                "blue" => "4",
                _ => null
            };
            if (dir == null)
                return;
            Speak(dir);
            ExitSubmenu();
            ExitSubmenu();
            _holding = false;
            Solve();
            return;
        }

        var btn = CommandMatcher().Match(command);
        var color = btn.Groups[1].Value;
        var label = btn.Groups[2].Value;

        var todo = UncertainCondition<Action>.Of(color == "blue" && label == "abort", Hold)
            | (label == "detonate" & Edgework.Batteries > 1, Tap)
            | (color == "white" & Edgework.HasIndicator("CAR", lit: true), Hold)
            | (Edgework.Batteries > 2 & Edgework.HasIndicator("FRK", lit: true), Tap)
            | (color == "yellow", Hold)
            | (color == "red" && label == "hold", Tap)
            | Hold;

        if (todo.IsCertain)
            todo.Value();
        else
            todo.Fill(() => ProcessCommand(command));
    }

    private void Tap()
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

    public override void Cancel()
    {
        if (_holding)
        {
            ExitSubmenu();
            _holding = false;
        }
    }

    public override void Reset() => _holding = false;

    [GeneratedRegex("(red|yellow|white|blue) (abort|detonate|hold|press)", RegexOptions.Compiled)]
    private static partial Regex CommandMatcher();
}
