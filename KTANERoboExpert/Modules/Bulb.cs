using KTANERoboExpert.Uncertain;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Text;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules;

public partial class Bulb : RoboExpertModule
{
    public override string Name => "Bulb";
    public override string Help => "blue opaque lit | red translucent unlt";
    private Grammar? _grammar, _subgrammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("red", "yellow", "white", "blue", "green", "purple")) + new Choices("opaque", "translucent") + new Choices("lit", "unlit"));
    private Grammar Subgrammar => _subgrammar ??= new(new Choices("yes", "no"));

    private Maybe<bool> _changed = default;
    private Maybe<string> _submenu = default;

    public override void ProcessCommand(string command)
    {
        if (_submenu.Exists)
        {
            _changed = command == "yes";
            command = _submenu.Item;
            _submenu = default;
            ExitSubmenu();
        }

        var btn = CommandMatcher().Match(command);
        var color = btn.Groups[1].Value;
        var opaque = btn.Groups[2].Value is "opaque";
        var on = btn.Groups[3].Value is "lit";

        var todo = Tree(1, color, opaque, on);
        if (todo.Where(c => !c.IsCertain).ToArray() is [var x, ..])
        {
            x.Fill(() => ProcessCommand(command), ExitSubmenu);
            return;
        }

        var cmd = todo.Select(c => c.Value).ToArray();
        bool skip = false;
        if (cmd.Contains( Command.Fence))
        {
            _changed = default;
            skip = true;
        }

        StringBuilder spoken = new();
        bool lastI = false, check = false;
        Maybe<bool> firstI = default;
        foreach (var c in cmd)
        {
            switch (c)
            {
                case Command.I:
                    lastI = true;
                    firstI = firstI.OrElse(true);
                    if (skip) break;
                    spoken.Append(" I");
                    break;
                case Command.O:
                    lastI = false;
                    firstI = firstI.OrElse(false);
                    if (skip) break;
                    spoken.Append(" O");
                    break;
                case Command.Screw:
                    if (skip) break;
                    spoken.Append(" Screw");
                    break;
                case Command.Unscrew:
                    if (skip) break;
                    spoken.Append(" Unscrew");
                    break;
                case Command.CheckOffI:
                    if (skip) break;
                    spoken.Append($". Did the light turn off during step {(firstI.Item! ? 1 : 2)}?");
                    check = true;
                    break;
                case Command.CheckOff1:
                    if (skip) break;
                    spoken.Append(". Did the light turn off during step 1?");
                    check = true;
                    break;
                case Command.CheckOn:
                    if (skip) break;
                    spoken.Append(on ? ". Is the light still on?" : ". Did the light turn on?");
                    check = true;
                    break;
                case Command.Fence:
                    skip = false;
                    break;
                case Command.Repeat:
                    if (skip) break;
                    if (lastI)
                        goto case Command.I;
                    goto case Command.O;
                case Command.RepeatFirst:
                    if (skip) break;
                    if (firstI.Item!)
                        goto case Command.I;
                    goto case Command.O;
                case Command.Unique:
                    if (skip) break;
                    if (lastI)
                        goto case Command.O;
                    goto case Command.I;
            }
        }

        Speak(spoken.ToString()[1..]);
        if (check)
        {
            _submenu = command;
            EnterSubmenu(Subgrammar);
        }
        else
        {
            ExitSubmenu();
            Solve();
        }
    }

    public override void Reset()
    {
        _submenu = default;
        _changed = default;
    }

    public override void Cancel()
    {
        if (_submenu.Exists)
            ExitSubmenu();
        Reset();
    }

    private Uncertain<Command>[] Tree(int step, string color, bool opaque, bool on, string? remember = null)
    {
        switch (step)
        {
            case 1:
                if (on && !opaque)
                    return [Command.I, .. Tree(2, color, opaque, on)];
                if (on && opaque)
                    return [Command.O, .. Tree(3, color, opaque, on)];
                return [Command.Unscrew, .. Tree(4, color, opaque, on)];

            case 2:
                return color switch
                {
                    "red" => [Command.I, Command.Unscrew, .. Tree(5, color, opaque, on)],
                    "white" => [Command.O, Command.Unscrew, .. Tree(6, color, opaque, on)],
                    _ => [Command.Unscrew, .. Tree(7, color, opaque, on)]
                };

            case 3:
                return color switch
                {
                    "green" => [Command.I, Command.Unscrew, .. Tree(6, color, opaque, on)],
                    "purple" => [Command.O, Command.Unscrew, .. Tree(5, color, opaque, on)],
                    _ => [Command.Unscrew, .. Tree(8, color, opaque, on)]
                };

            case 4:
                if (!Edgework.HasAnyIndicator("CAR", "IND", "MSA", "SND").IsCertain)
                    return [Uncertain<Command>.Of(Edgework.Indicators.Fill)];
                if (Edgework.HasAnyIndicator("CAR", "IND", "MSA", "SND").Value)
                    return [Command.I, .. Tree(9, color, opaque, on)];
                return [Command.O, .. Tree(10, color, opaque, on)];

            case 5:
                if (!_changed.Exists)
                    return [Command.CheckOff1];
                if (_changed.Item)
                    return [Command.Fence, Command.Repeat, Command.Screw];
                return [Command.Fence, Command.Unique, Command.Screw];

            case 6:
                if (!_changed.Exists)
                    return [Command.CheckOffI];
                if (_changed.Item)
                    return [Command.Fence, Command.RepeatFirst, Command.Screw];
                return [Command.Fence, Command.Repeat, Command.Screw];

            case 7:
                return color switch
                {
                    "green" => [Command.I, .. Tree(11, color, opaque, on, "SIG")],
                    "purple" => [Command.I, Command.Screw, .. Tree(12, color, opaque, on)],
                    "blue" => [Command.O, .. Tree(11, color, opaque, on, "CLR")],
                    _ => [Command.O, Command.Screw, .. Tree(13, color, opaque, on)]
                };

            case 8:
                return color switch
                {
                    "white" => [Command.I, .. Tree(11, color, opaque, on, "FRQ")],
                    "red" => [Command.I, Command.Screw, .. Tree(13, color, opaque, on)],
                    "yellow" => [Command.O, .. Tree(11, color, opaque, on, "FRK")],
                    _ => [Command.O, Command.Screw, .. Tree(12, color, opaque, on)]
                };

            case 9:
                return color switch
                {
                    "blue" => [Command.I, .. Tree(14, color, opaque, on)],
                    "green" => [Command.I, Command.Screw, .. Tree(12, color, opaque, on)],
                    "yellow" => [Command.O, .. Tree(15, color, opaque, on)],
                    "white" => [Command.O, Command.Screw, .. Tree(13, color, opaque, on)],
                    "purple" => [Command.Screw, Command.I, .. Tree(12, color, opaque, on)],
                    _ => [Command.Screw, Command.O, .. Tree(13, color, opaque, on)],
                };

            case 10:
                return color switch
                {
                    "purple" => [Command.I, .. Tree(14, color, opaque, on)],
                    "red" => [Command.I, Command.Screw, .. Tree(13, color, opaque, on)],
                    "blue" => [Command.O, .. Tree(15, color, opaque, on)],
                    "yellow" => [Command.O, Command.Screw, .. Tree(12, color, opaque, on)],
                    "green" => [Command.Screw, Command.I, .. Tree(13, color, opaque, on)],
                    _ => [Command.Screw, Command.O, .. Tree(12, color, opaque, on)],
                };

            case 11:
                return Edgework.HasIndicator(remember!).IsCertain ? Edgework.HasIndicator(remember!).Value ? [Command.I, Command.Screw] : [Command.O, Command.Screw] : [Uncertain<Command>.Of(Edgework.Indicators.Fill)];

            case 12:
                if (!_changed.Exists)
                    return [Command.CheckOn];
                if (_changed.Item)
                    return [Command.Fence, Command.I];
                return [Command.Fence, Command.O];

            case 13:
                if (!_changed.Exists)
                    return [Command.CheckOn];
                if (_changed.Item)
                    return [Command.Fence, Command.O];
                return [Command.Fence, Command.I];

            case 14:
                return opaque ? [Command.I, Command.Screw] : [Command.O, Command.Screw];

            case 15:
                return !opaque ? [Command.I, Command.Screw] : [Command.O, Command.Screw];

            default:
                throw new UnreachableException();
        }
    }

    private enum Command
    {
        I,
        O,
        Screw,
        Unscrew,
        CheckOffI,
        CheckOff1,
        CheckOn,
        Fence,
        Repeat,
        RepeatFirst,
        Unique
    }

    [GeneratedRegex("^(red|yellow|white|blue|green|purple) (opaque|translucent) (lit|unlit)$", RegexOptions.Compiled)]
    private static partial Regex CommandMatcher();
}
