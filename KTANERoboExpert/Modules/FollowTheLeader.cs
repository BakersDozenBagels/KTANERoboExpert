using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules;

public class FollowTheLeader : RoboExpertModule
{
    public override string Name => "Follow The Leader";
    public override string Help => "(answer the questions) | solve | undo | redo | reset";

    private Grammar? _yngrammar, _colorGrammar;
    public override Grammar Grammar => _yngrammar ??= new(new GrammarBuilder(new Choices("yes", "no", "solve")));
    private Grammar ColorGrammar => _colorGrammar ??= new(new GrammarBuilder(new Choices("red", "green", "white", "yellow", "blue", "black")));

    private static Maybe<Func<State, bool, State>> _fill = default;

    public override void ProcessCommand(string command)
    {
        if (command is "solve")
        {
            Speak("Done");
            _state.NewModule(new());
            ExitSubmenu();
            Solve();
            return;
        }
        if (command is "undo")
        {
            Speak("Undone");
            _state.Undo();
            return;
        }
        if (command is "redo")
        {
            Speak("Redone");
            _state.Redo();
            return;
        }
        if (command is "reset")
        {
            Speak("Reset");
            _state.Reset();
            return;
        }

        if (command is "red" or "green" or "white" or "yellow" or "blue" or "black")
        {
            _state.Current = _state.Current with { StartColor = command };
            Ask(_state.Current);
            return;
        }

        if (command is "yes" or "no" && _fill.Exists)
        {
            _state.Current = _fill.Item(_state.Current, command is "yes");
            _fill = default;
            Ask(_state.Current);
            return;
        }
    }

    private readonly UndoStack<State> _state;
    public FollowTheLeader() => _state = new(new());

    public override void Select() => Ask(_state.Current);

    private void Ask(State state)
    {
        var chain = UncertainCondition<int>.Of(Edgework.Ports.Count(x => x is Edgework.PortType.RJ45) > 0 & state._4to5, 0)
            | (state.WireAtBatteries, 1)
            | (state.WireAtSN, 2)
            | (Edgework.HasIndicator("CLR", lit: true), 3)
            | 4;

        if (!chain.IsCertain)
        {
            ExitSubmenu();
            EnterSubmenu(Grammar);
            chain.Fill(Select);
            return;
        }

        if (chain.Value is 3)
        {
            Speak("Cut in descending order");
            ExitSubmenu();
            Solve();
            return;
        }

        if (!state.StartColor.Exists)
        {
            Speak("Cut that wire. What color is it?");
            ExitSubmenu();
            EnterSubmenu(ColorGrammar);
            return;
        }

        if (!Edgework.SerialNumber.IsCertain)
        {
            Edgework.SerialNumber.Fill(Select);
            return;
        }

        var ix = (Edgework.SerialNumberLetters()[0].Value + (state.StartColor.Item is "red" or "green" or "white" ? 12 : 1) * state.StepsDone) % 13;
        if (!state.Rules[ix].IsCertain)
        {
            ExitSubmenu();
            EnterSubmenu(Grammar);
            state.Rules[ix].Fill(null!);
            return;
        }

        Speak(state.Rules[ix].Value ? "Cut" : "Skip");

        bool extra = false;
        if ((Edgework.SerialNumberLetters()[0].Value + (state.StartColor.Item is "red" or "green" or "white" ? 12 : 1) * (state.StepsDone + 1)) % 13 is (2 or 7) and var w)
        {
            Speak(w is 7 ^ state.Rules[ix].Value ? "Cut" : "Skip");
            extra = true;
        }

        if (state.StepsDone is >= 10 || (extra && state.StepsDone is 9))
        {
            _state.NewModule(new());
            ExitSubmenu();
            Solve();
            return;
        }

        _state.Current = state with { StepsDone = state.StepsDone + 1 + (extra ? 1 : 0) };
        Ask(_state.Current);
    }

    public override void Reset()
    {
        _state.Clear();
        _fill = default;
    }

    private record class State(UncertainBool _4to5, UncertainBool WireAtBatteries, UncertainBool RawWireAtSN, Maybe<string> StartColor, int StepsDone, UncertainBool[] Rules)
    {
        public UncertainBool WireAtSN => ((Edgework.Batteries == Edgework.SerialNumberDigits()[0].Into(0, 9)) & WireAtBatteries) | RawWireAtSN;

        public State() : this(default!, default!, default!, default, default, default!)
        {
            _4to5 = UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { _4to5 = b }); Speak("Wire from 4 to 5?"); });
            WireAtBatteries = UncertainBool.Of((_, __) => { if (!Edgework.Batteries.IsCertain) { Edgework.Batteries.Fill(_, __); return; } _fill = new((s, b) => s with { WireAtBatteries = b }); Speak("Wire from " + Edgework.Batteries.Value + "?"); });
            RawWireAtSN = UncertainBool.Of((_, __) => { if (!Edgework.SerialNumber.IsCertain) { Edgework.SerialNumber.Fill(_, __); return; } _fill = new((s, b) => s with { RawWireAtSN = b }); Speak("Wire from " + Edgework.SerialNumberDigits()[0].Value + "?"); });

            Rules = [
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..0], !b, ..Rules![1..]] }); Speak("Previous wire is yellow blue or green?"); }),
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..1], b, ..Rules![2..]] }); Speak("Previous wire leads to even plug?"); }),
                true,
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..3], b, ..Rules![4..]] }); Speak("Previous wire is red blue or black?"); }),
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..4], b, ..Rules![5..]] }); Speak("Two of previous three wires share color?"); }),
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..5], b, ..Rules![6..]] }); Speak("Exactly one of previous two wires matches this?"); }),
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..6], b, ..Rules![7..]] }); Speak("Previous wire is yellow white or green?"); }),
                false,
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..8], b, ..Rules![9..]] }); Speak("Previous wire skips a plug?"); }),
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..9], !b, ..Rules![10..]] }); Speak("Previous wire is white black or red?"); }),
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..10], !b, ..Rules![11..]] }); Speak("Previous two wires are same color?"); }),
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..11], !b, ..Rules![12..]] }); Speak("Previous wire leads to 6 or less?"); }),
                UncertainBool.Of((_, __) => { _fill = new((s, b) => s with { Rules = [..Rules![..12], !b, ..Rules![13..]] }); Speak("Both previous two wires are white or black?"); }),
            ];
        }
    }
}
