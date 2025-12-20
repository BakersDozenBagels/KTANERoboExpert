using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class ColoredSquares : RoboExpertModule
{
    public override string Name => "Colored Squares";
    public override string Help => "red 2 -> 4 -> 3 -> 2 -> ...";
    private Grammar? _grammar, _subgrammar;
    public override Grammar Grammar => _grammar ??= new(new Choices(new GrammarBuilder(new Choices("red", "blue", "green", "yellow", "magenta")) + new Choices(Numbers.ToArray()), "undo", "redo", "reset"));
    private Grammar Subgrammar => _subgrammar ??= new(new Choices([.. Numbers, "undo", "redo", "reset"]));

    private readonly UndoStack<(Color color, int presses)> _state = new((Color.None, 0));

    private static new string[] Numbers => [.. Enumerable.Range(1, 16).Select(i => i.ToString())];

    public override void ProcessCommand(string command)
    {
        if (command is "undo")
        {
            var b = _state.Current.color is Color.None;
            var u = _state.Undo();
            if (u.Exists)
            {
                Speak("Undone to " + u.Item.presses + " presses");
                if (b && u.Item.color is not Color.None)
                {
                    ExitSubmenu();
                    EnterSubmenu(Subgrammar);
                }
                if (!b && u.Item.color is Color.None)
                {
                    ExitSubmenu();
                    EnterSubmenu(Grammar);
                }
            }
            else
                Speak("Nothing to undo");
            return;
        }
        if (command is "redo")
        {
            var b = _state.Current.color is Color.None;
            var u = _state.Redo();
            if (u.Exists)
            {
                Speak("Redone to " + u.Item.presses + " presses");
                if (b && u.Item.color is not Color.None)
                {
                    ExitSubmenu();
                    EnterSubmenu(Subgrammar);
                }
                if (!b && u.Item.color is Color.None)
                {
                    ExitSubmenu();
                    EnterSubmenu(Grammar);
                }
            }
            else
                Speak("Nothing to redo");
            return;
        }
        if (command is "reset")
        {
            var b = _state.Current.color is Color.None;
            _state.Reset();
            Speak("Reset");
            if (!b && _state.Current.color is Color.None)
            {
                ExitSubmenu();
                EnterSubmenu(Grammar);
            }
            return;
        }

        if (_state.Current.color is Color.None)
        {
            var parts = command.Split(' ');
            var c = Enum.Parse<Color>(parts[0], true);
            var i = int.Parse(parts[1]);
            if (i is not (1 or 2))
            {
                Speak("Pardon?");
                return;
            }
            _state.Current = (c, i);
            ExitSubmenu();
            EnterSubmenu(Subgrammar);
        }
        else
        {
            var i = int.Parse(command);
            if (_state.Current.presses + i > 16 || i is 0)
            {
                Speak("Pardon?");
                return;
            }
            _state.Current = (Solution, _state.Current.presses + i);
        }

        if (_state.Current.presses is not 16)
            Speak(_state.Current.presses + " " + Solution);
        if (_state.Current.presses is 14 or 15 or 16)
        {
            ExitSubmenu();
            Solve();
            _state.Undo();
            _state.NewModule();
        }
    }

    public override void Select()
    {
        if (_state.Current.color is not Color.None)
        {
            ExitSubmenu();
            EnterSubmenu(Subgrammar);
            Speak("For " + _state.Current.presses + " pressed, " + Solution);
        }
        else
            base.Select();
    }

    public override void Reset() => _state.Clear();

    private static readonly Dictionary<Color, Color[]> _table = new()
    {
        [Color.Red] = [Color.Blue, Color.Row, Color.Yellow, Color.Blue, Color.Yellow, Color.Magenta, Color.Green, Color.Magenta, Color.Column, Color.Green, Color.Red, Color.Column, Color.Row, Color.Red, Color.Column],
        [Color.Blue] = [Color.Column, Color.Green, Color.Magenta, Color.Green, Color.Row, Color.Red, Color.Row, Color.Red, Color.Yellow, Color.Column, Color.Yellow, Color.Blue, Color.Magenta, Color.Blue, Color.Row],
        [Color.Green] = [Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Blue, Color.Yellow, Color.Column, Color.Green, Color.Red, Color.Row, Color.Row, Color.Magenta, Color.Column, Color.Magenta, Color.Column],
        [Color.Yellow] = [Color.Yellow, Color.Magenta, Color.Row, Color.Column, Color.Magenta, Color.Green, Color.Blue, Color.Blue, Color.Green, Color.Red, Color.Column, Color.Red, Color.Yellow, Color.Row, Color.Row],
        [Color.Magenta] = [Color.Row, Color.Red, Color.Blue, Color.Red, Color.Column, Color.Column, Color.Magenta, Color.Yellow, Color.Row, Color.Magenta, Color.Green, Color.Yellow, Color.Blue, Color.Green, Color.Column],
        [Color.Row] = [Color.Green, Color.Column, Color.Red, Color.Row, Color.Red, Color.Blue, Color.Yellow, Color.Column, Color.Magenta, Color.Blue, Color.Magenta, Color.Row, Color.Green, Color.Yellow, Color.Row],
        [Color.Column] = [Color.Magenta, Color.Yellow, Color.Column, Color.Magenta, Color.Green, Color.Row, Color.Red, Color.Row, Color.Blue, Color.Yellow, Color.Blue, Color.Green, Color.Red, Color.Column, Color.Column],
    };

    private Color Solution => _table[_state.Current.color][_state.Current.presses - 1];

    private enum Color
    {
        None,
        Red,
        Blue,
        Green,
        Yellow,
        Magenta,
        Row,
        Column
    }
}
