using System.Speech.Recognition;

namespace KTANERoboExpert.Modules.Bossy;

public class DividedSquares : RoboExpertModule
{
    public override string Name => "Divided Squares";
    public override string Help => "Red Blue | Divided 2 (side length) -> Red Blue White Green -> White Black";

    private static Grammar? _grammar, _subgrammar;
    private static readonly Grammar?[] _nbynCache = new Grammar?[10];
    public override Grammar Grammar => _grammar ??= new(new Choices(
        new GrammarBuilder(new Choices("red", "yellow", "green", "blue", "black", "white"), 2, 2),
        new GrammarBuilder("divided") + new Choices("2", "3", "4", "5", "6", "7", "8", "9", "10", "11")));
    private static Grammar NbyNGrammar(int n) => _nbynCache[n - 2] ??= new(new GrammarBuilder(new Choices("red", "yellow", "green", "blue", "black", "white"), n * n, n * n));
    private static Grammar Subgrammar => _subgrammar ??= new(new GrammarBuilder(new Choices("red", "yellow", "green", "blue", "black", "white"), 2, 2));

    private Maybe<int> _division = default;
    private int _lastIndex;
    private readonly List<(int s, int d, int p, int a, int b)> _notifs = [];

    private static readonly string[] _colors = ["red", "yellow", "green", "blue", "black", "white"];
    private static readonly int[][] _table = [[-1, 20, 21, 14, 12, 11], [9, -1, 25, 24, 27, 15], [4, 13, -1, 16, 0, 28], [2, 7, 1, -1, 23, 17], [10, 19, 29, 3, -1, 8], [6, 22, 5, 18, 26, -1]];

    public override void ProcessCommand(string command)
    {
        if (command.StartsWith("divided"))
        {
            _division = int.Parse(command[8..]);
            EnterSubmenu(NbyNGrammar(_division.Item));

            if (!Edgework.SerialNumber.IsCertain)
                Edgework.SerialNumber.Fill(() => { }, () => { ExitSubmenu(); _division = default; });
            else
                Speak("Go");

            return;
        }

        var colors = command.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => Array.IndexOf(_colors, s)).ToArray();

        if (colors.Length is 2)
        {
            var solves = _table[colors[0]][colors[1]];
            if (solves == -1)
            {
                Speak("Pardon?");
                return;
            }
            solves += _division.OrElse(1) * _division.OrElse(1) - 1;

            Speak("At " + solves + " solves");

            if ((Edgework.Solves >= solves).OrElse(false))
            {
                Speak("Solve now");
                Solve();
            }

            AddHook(solves, _lastIndex, _division.OrElse(1), colors[0], colors[1]);
            if (_division.Exists)
                ExitSubmenu();
            _division = default;
            ExitSubmenu();
            return;
        }

        HashSet<int> letters = [.. Edgework.SerialNumberLetters().Select(c => c - 'A' + 1)];
        int[] result = new int[_division.Item! * _division.Item!];
        for (int r = 0; r < _division.Item!; r++)
        {
            for (int c = 0; c < _division.Item! - 1; c++)
            {
                var h = _table[colors[r * _division.Item! + c]][colors[r * _division.Item! + c + 1]];
                if (letters.Contains(h))
                {
                    result[r * _division.Item! + c]++;
                    result[r * _division.Item! + c + 1]++;
                }
                var v = _table[colors[c * _division.Item! + r]][colors[(c + 1) * _division.Item! + r]];
                if (letters.Contains(v))
                {
                    result[c * _division.Item! + r]++;
                    result[(c + 1) * _division.Item! + r]++;
                }
            }
        }

        var indices = Enumerable.Range(0, _division.Item! * _division.Item!).Where(i => result[i] > 1).ToArray();

        if (indices.Length is not 1)
        {
            Speak("Pardon?");
            return;
        }

        _lastIndex = indices[0];
        Speak(Pos(indices[0], _division.Item!));
        ExitSubmenu();
        EnterSubmenu(Subgrammar);
    }

    private static string Pos(int ix, int d) => NATO.ElementAt(ix % d) + " " + ((ix / d) + 1);

    private void AddHook(int solves, int index, int division, int a, int b)
    {
        if (_notifs is [])
            OnSolve += CheckNotify;
        _notifs.Add((solves, division, index, a, b));
    }

    private void CheckNotify(string? _)
    {
        if (_notifs.Any(n => (Edgework.Solves == n.s).OrElse(false)))
        {
            var s = Edgework.Solves;
            Interrupt(yield =>
            {
                foreach (var n in _notifs.Where(n => (s == n.s).OrElse(false)))
                {
                    string add = _colors[n.a] + " " + _colors[n.b];
                    if (n.d != 1)
                        add = n.d + " by " + n.d + " at " + Pos(n.p, n.d) + " is " + add;
                    Speak("Divided Squares is ready: " + add);
                }
                yield();
            });
        }
    }

    public override void Cancel()
    {
        if (_division.Exists)
        {
            _division = default;
            ExitSubmenu();
        }
    }

    public override void Reset()
    {
        if (_notifs is not [])
            OnSolve -= CheckNotify;
        _notifs.Clear();
        _division = default;
    }
}
