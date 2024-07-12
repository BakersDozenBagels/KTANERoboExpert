using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class WireSequence : RoboExpertModule
{
    public override string Name => "Wire Sequence";
    public override string Help => "Red A Blue B";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices(new GrammarBuilder(new Choices("red", "blue", "black") + new GrammarBuilder("to", 0, 1) + new Choices(NATO.Take(3).ToArray()), 1, 3), "undo", "redo", "reset"));

    private readonly UndoStack<State> _undo = new(default);

    public override void ProcessCommand(string command)
    {
        switch (command)
        {
            case "reset":
                Speak(_undo.Reset().Exists ? "Resetting" : "Nothing to reset");
                break;
            case "undo":
                {
                    Speak(_undo.Undo() is { Exists: true, Item.Stage: var s } ? "Undone to wire " + s : "Nothing to undo");
                    break;
                }
            case "redo":
                {
                    Speak(_undo.Redo() is { Exists: true, Item.Stage: var s } ? "Redone to wire " + s : "Nothing to redo");
                    break;
                }
            default:
                var parts = command
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(p => p != "to")
                    .Chunk(2)
                    .Select(p => (Color: p[0], Slot: " ab c".IndexOf(p[1][0])))
                    .ToArray();

                var newState = _undo.Current;
                List<string> commands = [];
                for (int i = 0; i < parts.Length; i++)
                {
                    switch (parts[i].Color)
                    {
                        case "red":
                            commands.Add((_red[newState.R] & parts[i].Slot) == 0 ? "r skip" : "r cut");
                            newState.R++;
                            break;
                        case "blue":
                            commands.Add((_blue[newState.B] & parts[i].Slot) == 0 ? "b skip" : "b cut");
                            newState.B++;
                            break;
                        case "black":
                            commands.Add((_black[newState.K] & parts[i].Slot) == 0 ? "k skip" : "k cut");
                            newState.K++;
                            break;
                        default:
                            throw new UnreachableException();
                    }
                }

                if (newState.Stage >= 10)
                {
                    _undo.NewModule();
                    ExitSubmenu();
                }
                else
                    _undo.Do(newState);

                Speak(commands.Conjoin());
                break;
        }
    }

    public override void Reset() => _undo.Reset();
    public override void Select() => Speak("Go on Wire Sequence wire " + _undo.Current.Stage);

    private record struct State(byte R, byte B, byte K)
    {
        public readonly int Stage => R + B + K + 1;
    }

    // A = 1
    // B = 2
    // C = 4
    private static readonly byte[] _red = [4, 2, 1, 5, 2, 5, 7, 3, 2];
    private static readonly byte[] _blue = [2, 5, 2, 1, 2, 6, 4, 5, 1];
    private static readonly byte[] _black = [7, 5, 2, 5, 2, 6, 3, 4, 4];
}
