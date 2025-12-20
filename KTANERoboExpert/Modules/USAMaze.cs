using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class USAMaze : RoboExpertModule
{
    public override string Name => "USA Maze";
    public override string Help => "Kentucky to Alaska | Alfa Lima to Papa Alfa";
    private Grammar? _grammar, _subgrammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(Names)) + "to" + new GrammarBuilder(new Choices(Names)));
    private Grammar Subgrammar => _subgrammar ??= new(new Choices(Names));

    private Maybe<string> _goal;

    public override void ProcessCommand(string command)
    {
        string start;
        if (!_goal.Exists)
        {
            var split = command.IndexOf(" to ");
            _goal = ProcessName(command[(split + 4)..]);
            start = ProcessName(command[..split]);
            ExitSubmenu();
            EnterSubmenu(Subgrammar);
        }
        else
            start = ProcessName(command);

        var sol = Solve(start, _goal.Item!);
        if (!sol.Exists)
            throw new UnreachableException();

        if (sol.Item.Length is 0)
        {
            Speak("Pardon?");
            return;
        }

        Speak(sol.Item.Select(c => c.ToString()).Conjoin());
    }

    private static Maybe<Shape[]> Solve(string start, string goal)
    {
        HashSet<string> done = [start];
        Queue<(string state, Shape[] path)> todo = new([(start, [])]);

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

    public override void Reset() => _goal = default;

    public override void Cancel()
    {
        if (_goal.Exists)
        {
            ExitSubmenu();
            Solve();
            _goal = default;
        }
    }

    private static string ProcessName(string s) => _abbreviations.TryGetValue(s, out var n) ? n : s.Split(' ').Select(s => s[0].ToString()).Conjoin("").ToUpperInvariant();
    private static string[] Names => [.. _abbreviations.Keys, .. _abbreviations.Values.Select(s => NATO.ElementAt(s[0] - 'A') + " " + NATO.ElementAt(s[1] - 'A'))];

    private static (string state, Shape shape)[] Neighbors(string state)
    {
        if (state is "HI" or "AK")
            return [_maze[state][(int)DateTime.Now.DayOfWeek]];

        var thing = _maze[state];
        if (thing[^1].dest is "??")
        {
            if (Neighbors("AK")[0].state == state && Neighbors("AK")[0].shape == thing[^1].shape)
                return [.. thing[..^1], ("AK", thing[^1].shape)];

            if (Neighbors("HI")[0].state == state && Neighbors("HI")[0].shape == thing[^1].shape)
                return [.. thing[..^1], ("HI", thing[^1].shape)];

            return thing[..^1];
        }
        return thing;
    }

    private static readonly Dictionary<string, string> _abbreviations = new()
    {
        ["Alaska"] = "AK",
        ["Alabama"] = "AL",
        ["Arkansas"] = "AR",
        ["Arizona"] = "AZ",
        ["California"] = "CA",
        ["Colorado"] = "CO",
        ["Connecticut"] = "CT",
        ["Delaware"] = "DE",
        ["Florida"] = "FL",
        ["Georgia"] = "GA",
        ["Hawaii"] = "HI",
        ["Iowa"] = "IA",
        ["Idaho"] = "ID",
        ["Illinois"] = "IL",
        ["Indiana"] = "IN",
        ["Kansas"] = "KS",
        ["Kentucky"] = "KY",
        ["Louisiana"] = "LA",
        ["Massachusetts"] = "MA",
        ["Maryland"] = "MD",
        ["Maine"] = "ME",
        ["Michigan"] = "MI",
        ["Minnesota"] = "MN",
        ["Missouri"] = "MO",
        ["Mississippi"] = "MS",
        ["Montana"] = "MT",
        ["North Carolina"] = "NC",
        ["North Dakota"] = "ND",
        ["Nebraska"] = "NE",
        ["New Hampshire"] = "NH",
        ["New Jersey"] = "NJ",
        ["New Mexico"] = "NM",
        ["Nevada"] = "NV",
        ["New York"] = "NY",
        ["Ohio"] = "OH",
        ["Oklahoma"] = "OK",
        ["Oregon"] = "OR",
        ["Pennsylvania"] = "PA",
        ["Rhode Island"] = "RI",
        ["South Carolina"] = "SC",
        ["South Dakota"] = "SD",
        ["Tennessee"] = "TN",
        ["Texas"] = "TX",
        ["Utah"] = "UT",
        ["Virginia"] = "VA",
        ["Vermont"] = "VT",
        ["Washington"] = "WA",
        ["Wisconsin"] = "WI",
        ["West Virginia"] = "WV",
        ["Wyoming"] = "WY",
    };

    private static readonly Dictionary<string, (string dest, Shape shape)[]> _maze = new()
    {
        ["AK"] = [("WA", Shape.Circle), ("CA", Shape.Square), ("SC", Shape.Trapezoid), ("DE", Shape.Parallelogram), ("RI", Shape.Diamond), ("ME", Shape.Triangle), ("ND", Shape.Heart)],
        ["AL"] = [("MS", Shape.Trapezoid), ("FL", Shape.Circle)],
        ["AR"] = [("TX", Shape.Triangle), ("LA", Shape.Circle), ("MS", Shape.Square), ("TN", Shape.Trapezoid)],
        ["AZ"] = [("NV", Shape.Parallelogram)],
        ["CA"] = [("OR", Shape.Circle), ("??", Shape.Square)],
        ["CO"] = [("OK", Shape.Parallelogram), ("KS", Shape.Diamond)],
        ["CT"] = [("MA", Shape.Heart)],
        ["DE"] = [("MD", Shape.Diamond), ("??", Shape.Parallelogram)],
        ["FL"] = [("AL", Shape.Circle), ("GA", Shape.Square)],
        ["GA"] = [("SC", Shape.Diamond), ("FL", Shape.Square)],
        ["HI"] = [("CA", Shape.Square), ("WA", Shape.Circle), ("ME", Shape.Triangle), ("RI", Shape.Diamond), ("DE", Shape.Parallelogram), ("SC", Shape.Trapezoid), ("TX", Shape.Star)],
        ["IA"] = [("SD", Shape.Circle), ("MN", Shape.Parallelogram), ("IL", Shape.Star)],
        ["ID"] = [("WA", Shape.Square), ("OR", Shape.Diamond), ("UT", Shape.Heart), ("WY", Shape.Triangle)],
        ["IL"] = [("WI", Shape.Heart), ("IA", Shape.Star), ("MO", Shape.Square), ("IN", Shape.Diamond)],
        ["IN"] = [("IL", Shape.Diamond), ("MI", Shape.Trapezoid)],
        ["KS"] = [("CO", Shape.Diamond), ("OK", Shape.Trapezoid)],
        ["KY"] = [("OH", Shape.Triangle), ("VA", Shape.Trapezoid)],
        ["LA"] = [("AR", Shape.Circle)],
        ["MA"] = [("NY", Shape.Trapezoid), ("CT", Shape.Heart), ("RI", Shape.Star), ("VT", Shape.Triangle)],
        ["MD"] = [("DE", Shape.Diamond), ("PA", Shape.Square)],
        ["ME"] = [("NH", Shape.Diamond), ("??", Shape.Triangle)],
        ["MI"] = [("IN", Shape.Trapezoid), ("OH", Shape.Parallelogram), ("WI", Shape.Star)],
        ["MN"] = [("IA", Shape.Parallelogram), ("WI", Shape.Triangle)],
        ["MO"] = [("NE", Shape.Star), ("IL", Shape.Square), ("TN", Shape.Parallelogram), ("OK", Shape.Heart)],
        ["MS"] = [("AR", Shape.Square), ("TN", Shape.Diamond), ("AL", Shape.Trapezoid)],
        ["MT"] = [("WY", Shape.Circle), ("SD", Shape.Square)],
        ["NC"] = [("VA", Shape.Circle)],
        ["ND"] = [("SD", Shape.Diamond), ("??", Shape.Heart)],
        ["NE"] = [("SD", Shape.Trapezoid), ("MO", Shape.Star)],
        ["NH"] = [("ME", Shape.Diamond), ("VT", Shape.Square)],
        ["NJ"] = [("PA", Shape.Trapezoid)],
        ["NM"] = [("OK", Shape.Square), ("TX", Shape.Circle)],
        ["NV"] = [("OR", Shape.Trapezoid), ("AZ", Shape.Trapezoid)],
        ["NY"] = [("MA", Shape.Trapezoid), ("PA", Shape.Circle)],
        ["OH"] = [("MI", Shape.Parallelogram), ("KY", Shape.Triangle), ("WV", Shape.Heart)],
        ["OK"] = [("NM", Shape.Square), ("CO", Shape.Parallelogram), ("KS", Shape.Trapezoid), ("MO", Shape.Heart)],
        ["OR"] = [("CA", Shape.Circle), ("NV", Shape.Trapezoid), ("ID", Shape.Diamond)],
        ["PA"] = [("WV", Shape.Star), ("MD", Shape.Square), ("NJ", Shape.Trapezoid), ("NY", Shape.Circle)],
        ["RI"] = [("MA", Shape.Star), ("??", Shape.Diamond)],
        ["SC"] = [("GA", Shape.Diamond), ("??", Shape.Trapezoid)],
        ["SD"] = [("MT", Shape.Square), ("ND", Shape.Diamond), ("NE", Shape.Trapezoid), ("IA", Shape.Circle)],
        ["TN"] = [("MO", Shape.Parallelogram), ("AR", Shape.Trapezoid), ("MS", Shape.Diamond), ("VA", Shape.Star)],
        ["TX"] = [("NM", Shape.Circle), ("AR", Shape.Triangle), ("??", Shape.Star)],
        ["UT"] = [("ID", Shape.Heart), ("WY", Shape.Star)],
        ["VA"] = [("KY", Shape.Trapezoid), ("TN", Shape.Star), ("NC", Shape.Circle)],
        ["VT"] = [("NH", Shape.Square), ("MA", Shape.Triangle)],
        ["WA"] = [("ID", Shape.Square), ("??", Shape.Circle)],
        ["WI"] = [("MN", Shape.Triangle), ("IL", Shape.Heart), ("MI", Shape.Star)],
        ["WV"] = [("OH", Shape.Heart), ("PA", Shape.Star)],
        ["WY"] = [("MT", Shape.Circle), ("ID", Shape.Triangle), ("UT", Shape.Star)],
    };

    private enum Shape
    {
        Circle,
        Square,
        Trapezoid,
        Parallelogram,
        Diamond,
        Triangle,
        Heart,
        Star
    }
}
