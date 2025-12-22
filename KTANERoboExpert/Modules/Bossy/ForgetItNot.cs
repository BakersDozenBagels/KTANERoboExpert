using KTANERoboExpert.Uncertain;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules.Bossy;

public partial class ForgetItNot : RoboExpertModule
{
    public override string Name => "Forget It Not";
    public override string Help => "Stage 2 is 4 | Module 2 stage 5 is 6 | 3 modules | go | go 2 stage 5";
    private Grammar? _grammar, _subgrammar;
    public override Grammar Grammar => _grammar ??= new(new Choices(
        new GrammarBuilder(new GrammarBuilder("module") + new Choices(Numbers.ToArray()), 0, 1) + "stage" + new Choices(Numbers.ToArray()) + "is" + new Choices(Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray()),
        new GrammarBuilder(new Choices(Numbers.ToArray())) + "modules",
        "go" + new GrammarBuilder(new Choices(Numbers.ToArray()), 0, 1)) + new GrammarBuilder("stage" + new GrammarBuilder(new Choices(Numbers.ToArray())), 0, 1));
    private Grammar Subgrammar => _subgrammar ??= new(new Choices(Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray()));

    private readonly List<List<UncertainInt>> _stages = [];
    private Maybe<int> _submenu = new();
    private Action? _submenuYield;

    public override void ProcessCommand(string command)
    {
        if (_submenu.Exists)
        {
            _stages[_submenu.Item].SparseSet(Edgework.Solves.Min!, int.Parse(command), () => UncertainInt.Unknown(AskStage));
            if (_submenu.Item == _stages.Count - 1)
            {
                _submenu = new();
                ExitSubmenu();
                Speak(command + ", noted");
                _submenuYield!();
            }
            else
            {
                _submenu = _submenu.Map(x => x + 1);
                Speak(command);
            }
            return;
        }

        if (GoRegex().Match(command) is { Success: true, Groups: [_, var indexs, var ss] })
        {
            if (!indexs.Success || !int.TryParse(indexs.Value, out var ix) || ix < 1) ix = 1;
            if (!ss.Success || !int.TryParse(ss.Value, out var stage) || stage < 1) stage = 1;

            if (_stages.Count < ix)
            {
                Speak("Pardon?");
                return;
            }

            foreach (var s in _stages[ix - 1].Skip(stage - 1))
            {
                if (s.IsCertain)
                    Speak(s.Value.ToString());
                else
                    Speak("Guess");
            }
        }
        else if (StageRegex().Match(command) is { Success: true, Groups: [_, var ixs, var stages, var digits] } && int.Parse(stages.Value) is var stage && int.Parse(digits.Value) is var digit)
        {
            if (stage < 1)
            {
                Speak("Pardon?");
                return;
            }

            if (_stages is [])
                OnSolve += HandleSolve;

            if (!ixs.Success || !int.TryParse(ixs.Value, out var ix) || ix < 1) ix = 1;

            _stages.SparseExpand(ix, () => []);

            _stages[ix - 1].SparseSet(stage - 1, digit, () => UncertainInt.Unknown(AskStage));
            Speak("Stage " + stage + " is " + digit);
            ExitSubmenu();
        }
        else if (ModuleRegex().Match(command) is { Success: true, Groups: [_, var counts] } && int.Parse(counts.Value) is var count)
        {
            if (count < 1)
            {
                Speak("Pardon?");
                return;
            }

            if (_stages is [])
                OnSolve += HandleSolve;
            _stages.SparseExpand(count, () => []);

            _submenu = new(0);
            Speak(count + " modules. Go on stage " + (Edgework.Solves.Min! + 1));
            EnterSubmenu(Subgrammar);
            _submenuYield = () => ExitSubmenu();
        }
    }

    private static void AskStage(Action a, Action? _) { }

    public void HandleSolve(string? _)
    {
        Interrupt(yield =>
        {
            _submenu = new(0);
            Speak("Forget It Not" + (_stages.Count is 1 ? "" : " " + (_submenu.Item + 1)) + " stage " + (Edgework.Solves.Min! + 1));
            EnterSubmenu(Subgrammar);
            _submenuYield = yield;
        });
    }

    public override void Cancel()
    {
        if (_submenuYield is { })
            _submenuYield();
        _submenu = new();
    }

    public override void Reset()
    {
        if (_stages is not [])
            OnSolve -= HandleSolve;
        _stages.Clear();
    }

    [GeneratedRegex(@"^go(?: (\d+))?(?: stage (\d+))?$")]
    private static partial Regex GoRegex();

    [GeneratedRegex(@"^(?:module (\d+) )?stage (\d+) is (\d)$")]
    private static partial Regex StageRegex();
    [GeneratedRegex(@"^(\d+) modules$")]
    private static partial Regex ModuleRegex();
}
