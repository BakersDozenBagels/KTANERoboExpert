using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class ComplicatedWires : RoboExpertModule
{
    public override string Name => "Complicated Wires";
    public override string Help => "red blue light star next white done";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("white", "red", "blue", "light", "star"), 1, 4) + new GrammarBuilder("next" + new GrammarBuilder(new Choices("white", "red", "blue", "light", "star"), 1, 4), 0, 5) + "done");

    public override void ProcessCommand(string command)
    {
        var chunks = command[..^5]
            .Split(" next ")
            .Select(ParseChunk)
            .ToArray();

        if (!chunks.All(c => c.Exists))
            return;

        RunCommands(chunks.Select(c => c.Item).ToArray());
    }

    private void RunCommands(Command[] commands)
    {
        if (commands.Contains(Command.SerialNumber) && !Edgework.SerialNumber.IsCertain)
            Edgework.SerialNumber.Fill(() => RunCommands(commands));
        else if (commands.Contains(Command.Batteries) && !Edgework.Batteries.IsCertain)
            Edgework.Batteries.Fill(() => RunCommands(commands));
        else if (commands.Contains(Command.Parallel) && !Edgework.Ports.IsCertain)
            Edgework.Ports.Fill(() => RunCommands(commands));
        else
        {
            Speak(commands.Select(c => c switch
            {
                Command.Cut => true,
                Command.Skip => false,
                Command.SerialNumber => Edgework.SerialNumberDigits().Last() % 2 == 0,
                Command.Batteries => Edgework.Batteries.Value! >= 2,
                Command.Parallel => Edgework.Ports.Value!.Any(p => p.Parallel),
                _ => throw new UnreachableException(),
            }).Select(b => b ? "cut" : "skip").Conjoin());
            ExitSubmenu();
            Solve();
        }
    }

    private static Maybe<Command> ParseChunk(string ch)
    {
        var parts = ch.Split(' ');
        var w = parts.Count(x => x == "white");
        var r = parts.Count(x => x == "red");
        var b = parts.Count(x => x == "blue");
        var l = parts.Count(x => x == "light");
        var s = parts.Count(x => x == "star");
        if (w is > 1 || r is > 1 || b is > 1 || l > 1 || s > 1 || w is 1 && (b is not 0 && r is not 0) || (w is 0 && r is 0 && b is 0))
            return new();

        return new((r == 1, b == 1, l == 1, s == 1) switch
        {
            (false, false, false, _) or
            (true, false, false, true) => Command.Cut,
            (false, false, true, false) or
            (false, true, false, true) or
            (true, true, true, true) => Command.Skip,
            (false, false, true, true) or
            (true, false, true, _) => Command.Batteries,
            (true, false, false, false) or
            (false, true, false, false) or
            (true, true, _, false) => Command.SerialNumber,
            (false, true, true, _) or
            (true, true, false, true) => Command.Parallel,
        });
    }

    private enum Command : byte
    {
        Cut,
        Skip,
        SerialNumber,
        Batteries,
        Parallel
    }
}
