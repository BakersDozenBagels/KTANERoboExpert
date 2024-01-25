﻿using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules;

public partial class Button : RoboExpertModule
{
    public override string Name => "Button";
    public override string Help => "red abort";
    private Grammar? _grammar, _subGrammar;
    public override Grammar Grammar
    {
        get
        {
            if (_grammar != null)
                return _grammar;

            var colors = new Choices();
            colors.Add("red");
            colors.Add("yellow");
            colors.Add("white");
            colors.Add("blue");

            var labels = new Choices();
            labels.Add("abort");
            labels.Add("detonate");
            labels.Add("hold");
            labels.Add("press");

            var button = new GrammarBuilder(colors);
            button.Append(labels);

            return _grammar = new Grammar(button);
        }
    }
    private Grammar SubGrammar
    {
        get
        {
            if (_subGrammar != null)
                return _subGrammar;

            var colors = new Choices();
            colors.Add("red");
            colors.Add("yellow");
            colors.Add("white");
            colors.Add("blue");

            return _subGrammar = new Grammar(colors);
        }
    }

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
            return;
        }

        var btn = CommandMatcher().Match(command);
        var color = btn.Groups[1].Value;
        var label = btn.Groups[2].Value;

        if (color == "blue" && label == "abort")
            Hold();
        else if (label == "detonate" && Edgework.Batteries == null)
            Fill(EdgeworkType.Batteries, command);
        else if (label == "detonate" && Edgework.Batteries > 1)
            Tap();
        else if (color == "white" && Edgework.Indicators == null)
            Fill(EdgeworkType.Indicators, command);
        else if (color == "white" && Edgework.Indicators!.Any(i => i.Lit && i.Label == "CAR"))
            Hold();
        else if (Edgework.Batteries == null && Edgework.Indicators == null)
            Fill(EdgeworkType.Batteries, command);
        else if (Edgework.Indicators == null && Edgework.Batteries > 2)
            Fill(EdgeworkType.Indicators, command);
        else if (Edgework.Batteries == null && Edgework.Indicators!.Any(i => i.Lit && i.Label == "CAR"))
            Fill(EdgeworkType.Batteries, command);
        else if (Edgework.Batteries > 2 && Edgework.Indicators!.Any(i => i.Lit && i.Label == "CAR"))
            Tap();
        else if (color == "yellow")
            Hold();
        else if (color == "red" && label == "hold")
            Tap();
        else
            Hold();
    }

    private void Fill(EdgeworkType type, string command)
    {
        RequestEdgeworkFill(type, () => ProcessCommand(command));
    }

    private void Tap()
    {
        Speak("Tap");
        ExitSubmenu();
    }

    private void Hold()
    {
        SpeakSSML("Hold<break time=\"500ms\"/>Strip color?");
        EnterSubmenu(SubGrammar);
        _holding = true;
    }

    public override void Cancel() => _holding = false;

    public override void Reset() => _holding = false;

    [GeneratedRegex("(red|yellow|white|blue) (abort|detonate|hold|press)", RegexOptions.Compiled)]
    private static partial Regex CommandMatcher();
}
