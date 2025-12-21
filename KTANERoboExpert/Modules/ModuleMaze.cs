using KTANERoboExpert.Uncertain;
using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public partial class ModuleMaze : RoboExpertModule
{
    public override string Name => "Module Maze";
    public override string Help => "Moon -> Zoo";
    private Grammar? _grammar, _subgrammar, _csSubgrammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(Names)));
    private Grammar Subgrammar => _subgrammar ??= new(new Choices("yes", "no"));
    private Grammar CSSubgrammar => _csSubgrammar ??= new(new Choices("green", "red", "blue", "yellow"));

    private Maybe<string> _goal, _disambiguate;

    public override void ProcessCommand(string command)
    {
        Maybe<string> st = default;
        if (_disambiguate.Exists)
        {
            if (!_goal.Exists)
            {
                _goal = Disambiguate(command);
                if (_goal.Exists)
                    Speak(_goal.Item);
                return;
            }
            else
            {
                st = Disambiguate(command);
                if (!st.Exists)
                    return;
            }
        }

        if (!_goal.Exists)
        {
            var n = GetName(command);
            if (!n.IsCertain)
            {
                n.Fill(() => { });
                return;
            }

            _goal = n.Value;
            Speak(_goal.Item!);
            return;
        }

        string start;
        if (st.Exists)
            start = st.Item;
        else
        {
            var s = GetName(command);
            if (!s.IsCertain)
            {
                s.Fill(() => { });
                return;
            }
            start = s.Value;
        }

        Speak(start);

        var sol = Solve(Position(start), Position(_goal.Item));
        if (!sol.Exists)
            throw new UnreachableException();

        if (sol.Item.Length is 0)
        {
            Speak("Pardon?");
            return;
        }

        Speak(sol.Item.Select(c => c.ToString()).Conjoin());
    }

    private static Maybe<Direction[]> Solve(int start, int goal)
    {
        HashSet<int> done = [start];
        Queue<(int state, Direction[] path)> todo = new([(start, [])]);

        while (todo.Count > 0)
        {
            var (cur, path) = todo.Dequeue();

            if (cur == goal)
                return path;

            foreach (var (ns, nh) in Neighbors(cur))
                if (done.Add(ns))
                    todo.Enqueue((ns, [.. path, nh]));
        }

        return default;
    }

    private static IEnumerable<(int, Direction)> Neighbors(int cur)
    {
        if (cur >= 20 && _connections[cur - 20].Contains(cur))
            yield return (cur - 20, Direction.Up);
        if (cur < 390 && _connections[cur].Contains(cur + 20))
            yield return (cur + 20, Direction.Down);
        if (cur % 20 is not 0 && _connections[cur - 1].Contains(cur))
            yield return (cur - 1, Direction.Left);
        if (cur % 20 is not 19 && _connections[cur].Contains(cur + 1))
            yield return (cur + 1, Direction.Right);
    }

    public override void Reset()
    {
        _goal = default;
        _disambiguate = default;
    }

    public override void Cancel()
    {
        if (_disambiguate.Exists)
        {
            ExitSubmenu();
            _disambiguate = default;
            _goal = default;
            return;
        }

        if (_goal.Exists)
        {
            ExitSubmenu();
            Solve();
            _goal = default;
        }
    }

    private static string[] Names => [.. _abbreviations.Keys.Distinct()];

    private static int Position(string name) => _positions[name][0] - 'A' + 20 * (int.Parse(_positions[name][1..]) - 1);
    private Uncertain<string> GetName(string rawName)
    {
        rawName = _abbreviations[rawName];
        if (rawName is "Anagrams" or "Word Scramble")
            return Ask(rawName, "Status light on the left?");
        if (rawName is "Colorful Madness" or "Colorful Insanity")
            return Ask(rawName, "20 buttons?");
        if (rawName is "Crazy Talk" or "Krazy Talk")
            return Ask(rawName, "With a k?");
        if (rawName is "Turn The Key" or "Turn The Keys")
            return Ask(rawName, "Two keys?");
        if (rawName is "Colored Squares" or "Decolored Squares" or "Discolored Squares" or "Unolored Squares" or "Variolored Squares")
            return Ask(rawName, "Top-left square?", CSSubgrammar);

        return rawName;
    }

    private Maybe<string> Disambiguate(string command)
    {
        ExitSubmenu();
        var it = _disambiguate.Item!;
        _disambiguate = default;

        if (it is "Anagrams" or "Word Scramble")
            return command is "yes" ? "Anagrams" : "Word Scramble";
        if (it is "Colorful Madness" or "Colorful Insanity")
            return command is "yes" ? "Colorful Madness" : "Colorful Insanity";
        if (it is "Crazy Talk" or "Krazy Talk")
            return command is "yes" ? "Krazy Talk" : "Crazy Talk";
        if (it is "Turn The Key" or "Turn The Keys")
            return command is "yes" ? "Turn The Keys" : "Turn The Key";
        if (it is "Colored Squares" or "Decolored Squares" or "Discolored Squares" or "Unolored Squares" or "Variolored Squares")
        {
            if (command is "blue")
            {
                _disambiguate = "discosq";
                Speak("12 blue?");
                EnterSubmenu(Subgrammar);
                return default;
            }

            return command switch
            {
                "blue" => default(Maybe<string>),
                "red" => "Varicolored Squares",
                "yellow" => "Uncolored Squares",
                "green" => "Decolored Squares",
                _ => throw new UnreachableException()
            };
        }
        if (it is "discosq")
            return command is "yes" ? "Discolored Squares" : "Colored Squares";

        throw new UnreachableException();
    }

    private Uncertain<string> Ask(string name, string ask, Grammar? grammar = null) =>
        Uncertain<string>.Of((a, b) =>
            {
                _disambiguate = name;
                Speak(ask);
                EnterSubmenu(grammar ?? Subgrammar);
            });

    private enum Direction : byte
    {
        Up,
        Down,
        Left,
        Right,
    }
}
