using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules;

public partial class Memory : RoboExpertModule
{
    public override string Name => "Memory";
    public override string Help => "3 1 4 2 3 | reset | undo | redo";
    private Grammar? _grammar;
    public override Grammar Grammar
    {
        get
        {
            if (_grammar != null)
                return _grammar;

            var digit = new Choices("1", "2", "3", "4");

            var stage = new GrammarBuilder(digit, 5, 5);

            var choices = new Choices(stage);
            choices.Add("undo");
            choices.Add("redo");
            choices.Add("reset");

            return _grammar = new Grammar(choices);
        }
    }

    private readonly UndoStack<Stage[]> _undoHistory = new([]);
    private Stage[] Stages => _undoHistory.Current;

    public override void Select() => Speak("Go on memory stage " + (Stages.Length + 1));

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
                var m = CommandMatcher().Match(command);
                var stage = new Stage(
                        int.Parse(m.Groups[1].Value),
                        int.Parse(m.Groups[2].Value),
                        int.Parse(m.Groups[3].Value),
                        int.Parse(m.Groups[4].Value),
                        int.Parse(m.Groups[5].Value));

                if (!stage.Buttons.Order().SequenceEqual([1, 2, 3, 4]))
                {
                    Speak("Pardon?");
                    return;
                }

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
                switch (stage.Display)
                {
                    case 1:
                    case 2:
                        Position(2);
                        break;
                    case 3:
                        Position(3);
                        break;
                    case 4:
                        Position(4);
                        break;
                }
                break;
            case 1:
                switch (stage.Display)
                {
                    case 1:
                        Label(4);
                        break;
                    case 2:
                    case 4:
                        Position(Stages[^2].Press + 1);
                        break;
                    case 3:
                        Position(1);
                        break;
                }
                break;
            case 2:
                switch (stage.Display)
                {
                    case 1:
                        Label(Stages[^2].Buttons[Stages[^2].Press]);
                        break;
                    case 2:
                        Label(Stages[^3].Buttons[Stages[^3].Press]);
                        break;
                    case 3:
                        Position(3);
                        break;
                    case 4:
                        Label(4);
                        break;
                }
                break;
            case 3:
                switch (stage.Display)
                {
                    case 1:
                        Position(Stages[^4].Press + 1);
                        break;
                    case 2:
                        Position(1);
                        break;
                    case 3:
                    case 4:
                        Position(Stages[^3].Press + 1);
                        break;
                }
                break;
            case 4:
                switch (stage.Display)
                {
                    case 1:
                        Label(Stages[^5].Buttons[Stages[^5].Press]);
                        break;
                    case 2:
                        Label(Stages[^4].Buttons[Stages[^4].Press]);
                        break;
                    case 3:
                        Label(Stages[^2].Buttons[Stages[^2].Press]);
                        break;
                    case 4:
                        Label(Stages[^3].Buttons[Stages[^3].Press]);
                        break;
                }
                break;
        }
    }

    private void Position(int v)
    {
        Speak($"position {v}{(Stages.Length == 5 ? "" : $". on to stage {Stages.Length}")}");
        Stages[^1] = Stages[^1] with { Press = v - 1 };
        if (Stages.Length == 5)
        {
            // 5 stages done is not a valid state to ask about
            _undoHistory.Undo();
            _undoHistory.NewModule();
            ExitSubmenu();
        }
    }

    private void Label(int v) => Position(Array.IndexOf(Stages.Last().Buttons, v) + 1);

    public override void Reset()
    {
        _undoHistory.Clear();
    }

    [GeneratedRegex("([1-5]) ([1-5]) ([1-5]) ([1-5]) ([1-5])", RegexOptions.Compiled)]
    private static partial Regex CommandMatcher();

    private record struct Stage(int Display, int L1, int L2, int L3, int L4, int Press = -1, bool Skipped = false)
    {
        public readonly int[] Buttons => [L1, L2, L3, L4];
    }
}
