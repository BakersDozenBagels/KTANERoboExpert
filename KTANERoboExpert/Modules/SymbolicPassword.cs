using KTANERoboExpert.Modules.Vanilla;
using System.Diagnostics.CodeAnalysis;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class SymbolicPassword : RoboExpertModule
{
    public override string Name => "Symbolic Password";
    public override string Help => "flat 6 then trident then flat 6 then pilcrow then smiley then euro";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(Keypad.SymbolNames.Keys.ToArray()).ToGrammarBuilder() + "then", 5, 5) + new Choices(Keypad.SymbolNames.Keys.ToArray()));

    public override void ProcessCommand(string command)
    {
        var symbols = command.Split(" then ").Select(x => Keypad.SymbolNames[x]).ToArray();
        var s = symbols.Order().ToArray();

        List<(int r, int c)> valid = [];
        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 5; c++)
                if (Matches(s, r, c))
                    valid.Add((r, c));

        if (valid.Count is not 1)
        {
            Speak("Pardon?");
            return;
        }

        var (row, col) = valid[0];

        var goal = Get(row, col);
        var sol = Solve(symbols, goal);

        if (!sol.Exists || sol.Item.Length is 0)
        {
            Speak("Pardon?");
            return;
        }

        Speak(sol.Item.Select(c => c.ToString()).Conjoin(", "));
        ExitSubmenu();
        Solve();
    }

    private static Maybe<Move[]> Solve(int[] start, int[] goal)
    {
        var eq = new Eq();
        HashSet<int[]> done = new(eq) { start };
        Queue<(int[] state, Move[] path)> todo = new([(start, [])]);

        while (todo.Count > 0)
        {
            var (cur, path) = todo.Dequeue();

            if (eq.Equals(cur, goal))
                return path;

            foreach (var (ns, nh) in Neighbors(cur))
                if (done.Add(ns))
                    todo.Enqueue((ns, [.. path, nh]));
        }

        return default;
    }

    private class Eq : IEqualityComparer<int[]>
    {
        public bool Equals(int[]? x, int[]? y) => x!.SequenceEqual(y);
        public int GetHashCode([DisallowNull] int[] a) => HashCode.Combine(a[0], a[1], a[2], a[3], a[4], a[5]);
    }

    private static IEnumerable<(int[], Move)> Neighbors(int[] cur)
    {
        yield return ([cur[3], cur[1], cur[2], cur[0], cur[4], cur[5]], Move.Left);
        yield return ([cur[0], cur[4], cur[2], cur[3], cur[1], cur[5]], Move.Middle);
        yield return ([cur[0], cur[1], cur[5], cur[3], cur[4], cur[2]], Move.Right);
        yield return ([cur[1], cur[2], cur[0], cur[3], cur[4], cur[5]], Move.TopLeft);
        yield return ([cur[2], cur[0], cur[1], cur[3], cur[4], cur[5]], Move.TopRight);
        yield return ([cur[0], cur[1], cur[2], cur[4], cur[5], cur[3]], Move.BottomLeft);
        yield return ([cur[0], cur[1], cur[2], cur[5], cur[3], cur[4]], Move.BottomRight);
    }

    private static bool Matches(int[] symbols, int r, int c) => Get(r, c).Order().SequenceEqual(symbols);
    private static int[] Get(int r, int c) => [.. _table[r][c..(c + 3)], .. _table[r + 1][c..(c + 3)]];

    private static readonly int[][] _table = [
        [0, 7, 11, 15, 19, 15, 10],
        [1, 0, 12, 16, 18, 7, 9],
        [2, 6, 8, 17, 17, 23, 0],
        [3, 8, 13, 4, 20, 24, 2],
        [4, 9, 14, 13, 16, 19, 8],
        [5, 5, 2, 10, 21, 25, 7],
        [6, 10, 9, 18, 22, 26, 12],
    ];

    private enum Move
    {
        Left,
        Middle,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }
}
