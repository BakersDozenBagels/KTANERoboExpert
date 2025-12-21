using System.Diagnostics;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules.Bossy;

// TODO: fix the undoing here, it's so rough

public partial class ForgetAnyColor : RoboExpertModule
{
    public override string Name => "Forget Any Color";
    public override string Help => "[Module 2] 123456 Red Orange Yellow 789 Green | 3 Modules | Key 4 | Wrong | Undo | Redo";
    private Grammar? _grammar, _subgrammar;
    private static GrammarBuilder StageBuilder =>
        new GrammarBuilder(new Choices(Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray()), 6, 6)
        + new GrammarBuilder(new Choices("red", "orange", "yellow", "green", "cyan", "blue", "purple", "white"), 3, 3)
        + new GrammarBuilder(new Choices(Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray()), 3, 3)
        + new Choices("red", "orange", "yellow", "green", "cyan", "blue", "purple", "white");

    public override Grammar Grammar => _grammar ??= new(new Choices(
        new GrammarBuilder(new GrammarBuilder("module") + new Choices(Numbers.ToArray()), 0, 1) + StageBuilder,
        new GrammarBuilder(new Choices(Numbers.ToArray())) + "modules",
        "key" + new GrammarBuilder(new Choices(Numbers.ToArray())),
        "wrong", "undo", "redo"));
    private Grammar Subgrammar => _subgrammar ??= new(new Choices(StageBuilder, "undo", "redo", "wrong"));

    private readonly UndoStack<(List<Maybe<bool>> stages, int key, Maybe<int> ix, Maybe<int> submenu, Action? submenuYield)> _state = new(([], 4, default, default, null));
    private Action? _yield;

    public override void ProcessCommand(string command)
    {
        if (_state.Current.submenu.Exists && command != "undo" && command != "redo" && command != "wrong")
        {
            if (StageRegex().Match(command) is not { Success: true, Groups: [_, var ixs, var numbers, var cylinders, var nixies, var gears] })
                return;

            var ix = _state.Current.submenu.Item;
            var stages = _state.Current.stages.ToList().SparseExpanded(ix + 1, () => new());
            var sol = Solve(numbers.Value, cylinders.Value, nixies.Value, gears.Value, command);
            if (!sol.Exists)
                return;

            stages[ix] = (sol.Item, stages[ix]) switch
            {
                ( < 0, _) => false,
                (0, { Exists: false }) => true,
                (0, { Item: var i }) => !i,
                ( > 0, _) => true,
            };

            Speak(stages[ix].Item! ? "Right" : "Left");

            Maybe<int> nix;
            if (ix == _state.Current.stages.Count - 1)
            {
                nix = new();
                ExitSubmenu();
            }
            else
                nix = ix + 1;

            CheckHook(_state.Current.stages, stages);
            var yield = _state.Current.submenuYield;
            _state.Current = (stages, _state.Current.key, ix, nix, nix.Exists ? yield : null);

            if (yield is { })
                yield();
            if (_yield is { })
                _yield();

            return;
        }

        if (command == "wrong")
        {
            if (_state.Current.ix.Exists)
            {
                var s = _state.Current;
                s.stages[s.ix.Item] = !s.stages[s.ix.Item].Item;
                _state.Undo();
                _state.Current = s;
                Speak("Noted");
            }
        }
        else if (command == "undo")
        {
            var old = _state.Current.stages;
            if (!_state.Undo().Exists)
                Speak("Nothing to undo");
            else
                Speak("Done");
            CheckHook(old, _state.Current.stages);
        }
        else if (command == "redo")
        {
            var old = _state.Current.stages;
            if (!_state.Redo().Exists)
                Speak("Nothing to redo");
            else
                Speak("Done");
            CheckHook(old, _state.Current.stages);
        }
        else if (StageRegex().Match(command) is { Success: true, Groups: [_, var ixs, var numbers, var cylinders, var nixies, var gears] })
        {
            if (!ixs.Success || !int.TryParse(ixs.Value, out var ix)) ix = 1;
            ix--;

            if (ix < 0)
            {
                Speak("Pardon?");
                return;
            }

            var stages = _state.Current.stages.ToList().SparseExpanded(ix + 1, () => new());

            var sol = Solve(numbers.Value, cylinders.Value, nixies.Value, gears.Value, command);
            if (!sol.Exists)
                return;

            stages[ix] = (sol.Item, stages[ix]) switch
            {
                ( < 0, _) => false,
                (0, { Exists: false }) => true,
                (0, { Item: var i }) => !i,
                ( > 0, _) => true,
            };

            Speak(stages[ix].Item! ? "Right" : "Left");

            CheckHook(_state.Current.stages, stages);
            _state.Current = (stages, _state.Current.key, ix, default, null);

            ExitSubmenu();
        }
        else if (ModuleRegex().Match(command) is { Success: true, Groups: [_, var counts] } && int.Parse(counts.Value) is var count)
        {
            if (count < 1)
            {
                Speak("Pardon?");
                return;
            }

            var stages = _state.Current.stages.ToList().SparseExpanded(count, () => new());
            _state.Current = (stages, _state.Current.key, default, 0, ExitSubmenu);

            Speak(count + " modules. Go");
            EnterSubmenu(Subgrammar);
        }
        else if (KeyRegex().Match(command) is { Success: true, Groups: [_, var keys] } && int.Parse(keys.Value) is var key)
        {
            var stages = _state.Current.stages.ToList().SparseExpanded(1, () => new());
            CheckHook(_state.Current.stages, stages);
            _state.Current = (stages, key, default, default, null);
            Speak("Noted");
        }
    }

    private void CheckHook(List<Maybe<bool>> old, List<Maybe<bool>> @new)
    {
        if ((old.Count, @new.Count) is (0, > 0))
            OnSolve += HandleSolve;
        else if ((old.Count, @new.Count) is ( > 0, 0))
            OnSolve -= HandleSolve;
    }

    private Maybe<int> Solve(string number, string cylinder, string nixie, string gear, string command)
    {
        var exclude = gear switch
        {
            "red" => Edgework.Batteries,
            "orange" => Edgework.Indicators.Count,
            "yellow" => Edgework.PortPlates.Count,
            "green" => Edgework.SerialNumberDigits().Map(d => d.First()),
            "cyan" => Edgework.BatteryHolders,
            "blue" => Edgework.Indicators.Where(i => !i.Lit).Count,
            "purple" => Edgework.PortCount,
            "white" => Edgework.SerialNumberLetters().Count,
            _ => throw new UnreachableException()
        };

        if (!exclude.IsCertain)
        {
            exclude.Fill(() => ProcessCommand(command));
            return new();
        }

        int excl = (exclude.Value % 10) switch
        {
            0 => 6,
            >= 1 and <= 6 and var x => x,
            > 6 and var x => x - 6,
            _ => throw new UnreachableException()
        };

        var numbers = number.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        numbers.RemoveAt(excl - 1);

        string[] colors = ["red", "orange", "yellow", "green", "cyan", "blue", "purple", "white"];
        int[][] table = [[1, 7, 3], [6, 2, 8], [8, 5, 1], [5, 4, 6], [2, 6, 4], [7, 3, 5], [3, 1, 7], [4, 8, 2]];
        int[] adds = [.. cylinder.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select((c, i) => table[Array.IndexOf(colors, c)][i])];

        var goals = nixie.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

        int[][] patterns = [[0, 0, 0, 1, 2], [0, 1, 1, 1, 2], [0, 1, 2, 2, 2], [0, 1, 1, 2, 2], [0, 0, 1, 2, 2], [0, 0, 1, 1, 2]];

        static bool IsValid(int[] pattern, List<int> numbers, int[] adds, int[] goals) =>
            Enumerable.Range(0, 3).All(lmr => Enumerable.Range(0, 5).Where(i => pattern[i] == lmr).Sum(i => numbers[i] + adds[lmr]) % 10 == goals[lmr]);

        var sols = Enumerable.Range(0, 6).Where(i => IsValid(patterns[i], numbers, adds, goals)).ToArray();
        if (sols.Length is not 1)
        {
            Speak("Pardon?");
            return new();
        }

        return (sols[0] % 3) - 1;
    }

    public void HandleSolve(string? _)
    {
        if (Edgework.Solves.Min % _state.Current.key == 0)
            Interrupt(yield =>
            {
                var s = _state.Current;
                s.submenu = 0;
                _yield = yield;
                _state.Undo();
                _state.Current = s;

                Speak("Forget Any Color" + (s.stages.Count is 1 ? "" : " " + (s.submenu.Item + 1)) + " go");
                EnterSubmenu(Subgrammar);
            });
    }

    public override void Cancel()
    {
        if (_yield is { })
            _yield();

        var s = _state.Current;
        s.submenu = default;
        _state.Undo();
        _state.Current = s;
    }

    public override void Reset()
    {
        if (_state.Current.stages.Count > 0)
            OnSolve -= HandleSolve;
        _state.Clear();
    }


    [GeneratedRegex(@"^(?:module (\d+) )?((?:\d ){6})((?:(?:red|orange|yellow|green|cyan|blue|purple|white) ){3})((?:\d ){3})(red|orange|yellow|green|cyan|blue|purple|white)$")]
    private static partial Regex StageRegex();
    [GeneratedRegex(@"^(\d+) modules$")]
    private static partial Regex ModuleRegex();
    [GeneratedRegex(@"^key (\d+)$")]
    private static partial Regex KeyRegex();
}
