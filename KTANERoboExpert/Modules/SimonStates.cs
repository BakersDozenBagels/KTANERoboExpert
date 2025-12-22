using System.Diagnostics.Contracts;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public partial class SimonStates : RoboExpertModule
{
    public override string Name => "Simon States";
    public override string Help => "red yellow blue | all | undo | redo | reset";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices(new GrammarBuilder(new Choices("Red", "Yellow", "Blue", "Green"), 1, 4), "All", "undo", "redo", "reset"));

    private readonly UndoStack<Stage[]> _undoHistory = new([]);
    private Stage[] Stages => _undoHistory.Current;
    private Colors[] Presses => [.. Stages.Select(s => s.Press)];
    private bool _submenu = false;

    public override void Select() => Speak("Go on Simon States flash " + (Stages.Length + 1));

    public override void ProcessCommand(string command)
    {
        switch (command)
        {
            case "reset":
                Speak(_undoHistory.Reset().Exists ? "Resetting" : "Nothing to reset");
                break;
            case "undo":
                {
                    Speak(_undoHistory.Undo() is { Exists: true, Item.Length: var len } ? "Undone to stage " + (len + 1) : "Nothing to undo");
                    break;
                }
            case "redo":
                {
                    Speak(_undoHistory.Redo() is { Exists: true, Item.Length: var len } ? "Redone to stage " + (len + 1) : "Nothing to redo");
                    break;
                }
            default:
                var flash = command.Split(' ').Select(Enum.Parse<Colors>).ToArray();

                if (flash.Distinct().Count() != flash.Length)
                {
                    Speak("Pardon?");
                    return;
                }

                if (_submenu)
                {
                    if (flash.Length > 1 || flash is [Colors.All])
                    {
                        Speak("Pardon?");
                        return;
                    }

                    _submenu = false;
                    Stages[^1].TopLeft = flash[0];
                    RunStage(Stages[^1], Stages.Length - 1);
                    return;
                }

                var stage = new Stage(flash.Aggregate((a, b) => a | b), Colors.None, Colors.None);
                if (Stages.Length is > 0)
                    stage.TopLeft = Stages[^1].TopLeft;

                var newState = new Stage[Stages.Length + 1];
                Array.Copy(Stages, newState, newState.Length - 1);
                newState[^1] = stage;
                _undoHistory.Do(newState);

                RunStage(stage, newState.Length - 1);
                break;
        }
    }

    private void RunStage(Stage stage, int numDone)
    {
        switch (numDone)
        {
            case 0:
                if (PopCount(stage.Flash) is 1)
                    Color(stage.Flash);
                else if (PopCount(stage.Flash) is 2 && stage.Flash.HasFlag(Colors.Blue))
                    Prio(stage, stage.Flash);
                else if (PopCount(stage.Flash) is 2)
                    Color(Colors.Blue);
                else if (PopCount(stage.Flash) is 3 && stage.Flash.HasFlag(Colors.Red))
                    Prio(stage, stage.Flash, low: true);
                else if (PopCount(stage.Flash) is 3)
                    Color(Colors.Red);
                else
                    Prio(stage, Colors.All, second: true);
                break;

            case 1:
                if (stage.Flash is (Colors.Red | Colors.Blue))
                    Prio(stage, Colors.Green | Colors.Yellow);
                else if (PopCount(stage.Flash) is 2)
                    Prio(stage, Not(stage.Flash), low: true);
                else if (PopCount(stage.Flash) is 1 && stage.Flash is not Colors.Blue)
                    Color(Colors.Blue);
                else if (PopCount(stage.Flash) is 1)
                    Color(Colors.Yellow);
                else if (stage.Flash is Colors.All)
                    Color(Presses[0]);
                else
                    Color(Not(stage.Flash));
                break;

            case 2:
                if (PopCount(stage.Flash) is 3 && Presses.Any(c => stage.Flash.HasFlag(c)))
                    Prio(stage, ((Colors[])[Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow]).Where(c => stage.Flash.HasFlag(c) && !Presses.Contains(c)).Aggregate((a, b) => a | b));
                else if (PopCount(stage.Flash) is 3)
                    Prio(stage, stage.Flash);
                else if (PopCount(stage.Flash) is 2 && ((Colors[])[Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow]).Where(c => stage.Flash.HasFlag(c)).All(Presses.Contains))
                    Prio(stage, Not(stage.Flash), low: true);
                else if (PopCount(stage.Flash) is 2)
                    Color(Presses[0]);
                else if (PopCount(stage.Flash) is 1)
                    Color(stage.Flash);
                else
                    Prio(stage, Colors.All, low: true, second: true);
                break;

            case 3:
                if (PopCount(Presses.Aggregate((a, b) => a | b)) is 3)
                    Color(Not(Presses.Aggregate((a, b) => a | b)));
                else if (PopCount(stage.Flash) is 3 && ((Colors[])[Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow]).Where(c => stage.Flash.HasFlag(c)).Count(c => !Presses.Contains(c)) is 1)
                    Color(((Colors[])[Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow]).Where(c => stage.Flash.HasFlag(c)).First(c => !Presses.Contains(c)));
                else if (PopCount(stage.Flash) is >= 3)
                    Prio(stage, Colors.All, low: true);
                else if (PopCount(stage.Flash) is 1)
                    Color(stage.Flash);
                else
                    Color(Colors.Green);
                break;
        }
    }

    private void Prio(Stage stage, Colors check, bool low = false, bool second = false)
    {
        if (PopCount(check) is 1)
        {
            Color(check);
            return;
        }

        if (stage.TopLeft is Colors.None)
        {
            Speak("Top left color?");
            _submenu = true;
            return;
        }

        var order = _table[stage.TopLeft]!.ToArray();
        if (low)
            order.Reverse();

        if (second)
            Color(order.Where(c => check.HasFlag(c)).ElementAt(1));
        else
            Color(order.First(c => check.HasFlag(c)));
    }

    private static readonly Dictionary<Colors, Colors[]> _table = new()
    {
        [Colors.Red] = [Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow],
        [Colors.Yellow] = [Colors.Blue, Colors.Yellow, Colors.Red, Colors.Green],
        [Colors.Green] = [Colors.Green, Colors.Red, Colors.Yellow, Colors.Blue],
        [Colors.Blue] = [Colors.Yellow, Colors.Green, Colors.Blue, Colors.Red],
    };

    private void Color(Colors v)
    {
        Stages[^1] = Stages[^1] with { Press = v };

        bool skip = false;
        if (Stages.Length is 3 && PopCount(Presses.Aggregate((a, b) => a | b)) is 3)
        {
            Speak(((Colors[])[.. Presses, .. Presses, Not(Presses.Aggregate((a, b) => a | b))]).Select(c => c.ToString()).Conjoin(" "));
            skip = true;
        }
        else
            Speak($"{Presses.Select(c => c.ToString()).Conjoin(" ")}{(Stages.Length == 4 ? "" : $". on to flash {Stages.Length + 1}")}");
        if (Stages.Length == 4 || skip)
        {
            // 4 stages done is not a valid state to ask about
            _undoHistory.Undo();
            _undoHistory.NewModule();
            ExitSubmenu();
            Solve();
        }
    }

    public override void Cancel() => _submenu = false;

    public override void Reset()
    {
        _undoHistory.Clear();
        _submenu = false;
    }

    private record struct Stage(Colors Flash, Colors Press, Colors TopLeft) { }
    [Flags]
    private enum Colors : byte
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 4,
        Yellow = 8,
        All = 15
    }

    [Pure]
    private static Colors Not(Colors c) => c ^ Colors.All;
    [Pure]
    private static int PopCount(Colors c) => ((Colors[])[Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow]).Sum(d => c.HasFlag(d) ? 1 : 0);
}
