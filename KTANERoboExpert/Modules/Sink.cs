using KTANERoboExpert.Uncertain;
using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class Sink : RoboExpertModule
{
    public override string Name => "Sink";
    public override string Help => "yes | no";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices("yes", "no"));

    private Maybe<int> _rix = default;
    private UncertainBool[] DefaultRules => [
        UncertainBool.Of((a, _) => { Speak("Gold knobs?"); _rix = 0; }),
        UncertainBool.Of((a, _) => { Speak("Steel faucet?"); _rix = 1; }),
        UncertainBool.Of((a, _) => { Speak("Copper drain?"); _rix = 2; })
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
        var row = UncertainCondition<(int[], bool)>.Of(Edgework.Batteries < 2, ([2, 1, 4], true))
            | (Edgework.Batteries > 1 & Edgework.Batteries < 4, ([3, 6, 2], false))
            | (Edgework.Batteries > 3 & Edgework.Batteries < 6, ([5, 3, 1], false))
            | ([5, 6, 4], true);

        UncertainBool[] conds =
        [
            Edgework.HasIndicator("NSA", lit: false),
            Edgework.SerialNumberVowels().Count != 0,
            ..Rules,
            Edgework.PortPlates.Where(pl => pl.RJ45).Count != 0
        ];

        var total = row.FlatMap(v => v.Item1.Select(i => conds[i - 1].IsCertain ? true : conds[i - 1]).Aggregate((a, b) => a & b));
        if (!total.IsCertain)
        {
            total.Fill(Select, ExitSubmenu);
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
