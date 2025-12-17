using KTANERoboExpert.Uncertain;
using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public partial class Sink : RoboExpertModule
{
    public override string Name => "Sink";
    public override string Help => "yes | no";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices("yes", "no"));

    private Maybe<int> _rix = default;
    private UncertainBool[] DefaultRules => [
        new((a, _) => { Speak("Gold knobs?"); _rix = 0; }),
        new((a, _) => { Speak("Steel faucet?"); _rix = 1; }),
        new((a, _) => { Speak("Copper pipes?"); _rix = 2; })
    ];
    private UncertainBool[] Rules { get => field ??= DefaultRules; set; }

    public override void ProcessCommand(string command)
    {
        if (!_rix.Exists) throw new UnreachableException();
        Rules[_rix.Item] = command == "yes";
        Select();
    }

    public override void Select()
    {
        var row = new UncertainCondition<(int[], bool)>(Edgework.Batteries < 2, ([2, 1, 4], true))
            | (Edgework.Batteries > 1 & Edgework.Batteries < 4, ([3, 6, 2], false))
            | (Edgework.Batteries > 3 & Edgework.Batteries < 6, ([5, 3, 1], false))
            | ([5, 6, 4], true);

        if (!row.IsCertain)
        {
            row.Fill(Select);
            return;
        }

        UncertainBool[] conds =
        [
            Edgework.HasIndicator("NSA", lit: false),
            Edgework.SerialNumberVowels().Count != 0,
            ..Rules,
            Edgework.Ports.Where(pl => pl.RJ45).Count != 0
        ];

        var total = row.Value.Item1.Select(i => conds[i - 1].AsUncertainBool()).Aggregate((a, b) => a & b);
        if (!total.IsCertain)
        {
            total.Fill(Select);
            return;
        }

        Speak(row.Value.Item1.Select(i => conds[i - 1].Value! ^ row.Value.Item2 ? "cold" : "hot").Conjoin());
        Reset();
        ExitSubmenu();
        Solve();
    }

    public override void Cancel() => Reset();
    public override void Reset() => Rules = DefaultRules;
}
