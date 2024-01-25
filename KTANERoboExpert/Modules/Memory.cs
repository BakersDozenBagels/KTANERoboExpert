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

    private int? _stackPointer = null;
    private readonly List<Stage> _stageHistory = [];

    public override bool Select()
    {
        Speak("Go on memory stage " + ((_stackPointer ?? _stageHistory.Count) % 5 + 1));
        return true;
    }

    public override void ProcessCommand(string command)
    {
        if (command == "reset")
        {
            _stackPointer ??= _stageHistory.Count;
            if (_stackPointer % 5 == 0)
                return;
            _stageHistory.RemoveRangeQuietly(_stackPointer.Value + 1, _stageHistory.Count);
            int additional = 4 - (_stackPointer.Value % 5);
            for (int i = 0; i < additional; i++)
                _stageHistory.Add(new(0, 0, 0, 0, 0, -1, true));
            _stageHistory.Add(new());
            _stackPointer = null;

            Speak("Resetting");
        }
        else if (command == "undo")
        {
            if (_stageHistory.Count == 0 || _stackPointer == -1)
                return;
            if (_stackPointer == null)
                _stackPointer = _stageHistory.Count - 2;
            else
                _stackPointer--;
            while (_stackPointer > -1 && _stageHistory[_stackPointer.Value].Skipped)
                _stackPointer--;
            Speak("Undone to stage " + ((_stackPointer + 1) % 5 + 1));
        }
        else if (command == "redo")
        {
            if (_stackPointer == null)
                return;
            _stackPointer++;
            while (_stageHistory[_stackPointer.Value].Skipped)
                _stackPointer++;
            if (_stackPointer == _stageHistory.Count - 1)
                _stackPointer = null;
            Speak("Redone to stage " + (((_stackPointer ?? _stageHistory.Count - 1) + 1) % 5 + 1));
        }
        else
        {
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

            if (_stackPointer != null)
                _stageHistory.RemoveRangeQuietly(_stackPointer.Value + 1, _stageHistory.Count);
            _stackPointer = null;
            _stageHistory.Add(stage);

            RunStage(stage, (_stageHistory.Count - 1) % 5);
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
                        Position(_stageHistory[^2].Press + 1);
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
                        Label(_stageHistory[^2].Buttons[_stageHistory[^2].Press]);
                        break;
                    case 2:
                        Label(_stageHistory[^3].Buttons[_stageHistory[^3].Press]);
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
                        Position(_stageHistory[^4].Press + 1);
                        break;
                    case 2:
                        Position(1);
                        break;
                    case 3:
                    case 4:
                        Position(_stageHistory[^3].Press + 1);
                        break;
                }
                break;
            case 4:
                switch (stage.Display)
                {
                    case 1:
                        Label(_stageHistory[^5].Buttons[_stageHistory[^5].Press]);
                        break;
                    case 2:
                        Label(_stageHistory[^4].Buttons[_stageHistory[^4].Press]);
                        break;
                    case 3:
                        Label(_stageHistory[^2].Buttons[_stageHistory[^2].Press]);
                        break;
                    case 4:
                        Label(_stageHistory[^3].Buttons[_stageHistory[^3].Press]);
                        break;
                }
                break;
        }
    }

    private void Position(int v)
    {
        Speak($"position {v}{(_stageHistory.Count % 5 == 0 ? "" : $". on to stage {_stageHistory.Count % 5 + 1}")}");
        _stageHistory[^1] = _stageHistory[^1] with { Press = v - 1 };
        if (_stageHistory.Count % 5 == 0)
            ExitSubmenu();
    }

    private void Label(int v) => Position(Array.IndexOf(_stageHistory.Last().Buttons, v) + 1);

    public override void Reset()
    {
        _stackPointer = null;
        _stageHistory.Clear();
    }

    [GeneratedRegex("([1-5]) ([1-5]) ([1-5]) ([1-5]) ([1-5])", RegexOptions.Compiled)]
    private static partial Regex CommandMatcher();

    private record struct Stage(int Display, int L1, int L2, int L3, int L4, int Press = -1, bool Skipped = false)
    {
        public readonly int[] Buttons => [L1, L2, L3, L4];
    }
}
