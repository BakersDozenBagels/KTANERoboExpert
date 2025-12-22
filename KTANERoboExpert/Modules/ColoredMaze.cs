using KTANERoboExpert.Uncertain;
using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public partial class ColoredMaze : RoboExpertModule
{
    public override string Name => "Colored Maze";
    public override string Help => "";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder("unused"));

    public override void ProcessCommand(string command) => throw new UnreachableException();

    public override void Select()
    {
        var start = Edgework.SerialNumberDigits()[0].Map(d => (d % 4 + 1) * 6).Into() + Edgework.SerialNumberLetters()[0].Map(l => (l - 'A' + 1) % 4 + 1).Into();
        if (!start.IsCertain)
        {
            start.Fill(Select, ExitSubmenu);
            return;
        }
        var sol = Solve(start.Value);
        if (!sol.Exists)
            throw new UnreachableException();

        Speak(sol.Item.Select(s => s.ToString()).Conjoin(", "));
        ExitSubmenu();
        Solve();
    }

    private static Maybe<Color[]> Solve(int start)
    {
        HashSet<int> done = [start];
        Queue<(int state, Color[] path)> todo = new([(start, [])]);

        while (todo.Count > 0)
        {
            var (cur, path) = todo.Dequeue();

            if (cur is 0 or 5 or 30 or 35)
                return path;

            foreach (var (ns, nh) in Neighbors(cur))
                if (done.Add(ns))
                    todo.Enqueue((ns, [.. path, nh]));
        }

        return default;
    }

    private static IEnumerable<(int, Color)> Neighbors(int cur)
    {
        if ((_mazeWalls[cur] & 0b0001) == 0)
            yield return (cur - 1, _mazeColors[cur - 1]);
        if ((_mazeWalls[cur] & 0b0010) == 0)
            yield return (cur + 6, _mazeColors[cur + 6]);
        if ((_mazeWalls[cur] & 0b0100) == 0)
            yield return (cur + 1, _mazeColors[cur + 1]);
        if ((_mazeWalls[cur] & 0b1000) == 0)
            yield return (cur - 6, _mazeColors[cur - 6]);
    }

    private static readonly Color[] _mazeColors = [
        Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Cyan, Color.Purple,
        Color.Green, Color.Orange, Color.Red, Color.Purple, Color.Cyan, Color.Yellow,
        Color.Purple, Color.Cyan, Color.Green, Color.Orange, Color.Red, Color.Yellow,
        Color.Orange, Color.Red, Color.Purple, Color.Yellow, Color.Green, Color.Cyan,
        Color.Cyan, Color.Yellow, Color.Purple, Color.Red, Color.Green, Color.Orange,
        Color.Purple, Color.Green, Color.Cyan, Color.Red, Color.Yellow, Color.Orange,
    ];

    // wall at 0b up right down left
    private static readonly byte[] _mazeWalls = [
        0b1001, 0b1100, 0b1001, 0b1000, 0b1000, 0b1100,
        0b0101, 0b0001, 0b0110, 0b0111, 0b0101, 0b0101,
        0b0101, 0b0011, 0b1110, 0b1001, 0b0110, 0b0101,
        0b0011, 0b1110, 0b1001, 0b0100, 0b1101, 0b0101,
        0b1001, 0b1010, 0b0110, 0b0111, 0b0011, 0b0100,
        0b0011, 0b1010, 0b1010, 0b1110, 0b1011, 0b0110,
    ];

    private enum Color : byte
    {
        Red,
        Orange,
        Yellow,
        Green,
        Cyan,
        Purple
    }
}
