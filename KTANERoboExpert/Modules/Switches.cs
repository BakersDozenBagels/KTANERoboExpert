using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class Switches : RoboExpertModule
{
    public override string Name => "Switches";
    public override string Help => "Up Up Up Up Up to Down Down Down Down Down";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("up", "down"), 5, 5) + "to" + new GrammarBuilder(new Choices("up", "down"), 5, 5));

    public override void ProcessCommand(string command)
    {
        var parts = command.Split();
        var sol = Solve(ToInt(parts[..5]), ToInt(parts[6..]));
        if (sol is [])
        {
            Speak("Pardon?");
            return;
        }
        Speak(sol.Select(x => (5 - x).ToString()).Conjoin());
        ExitSubmenu();
        Solve();
    }

    private static int ToInt(string[] s) => s.Select((s, i) => s == "up" ? 1 << (4 - i) : 0).Sum();

    private static int[] Solve(int start, int end)
    {
        if (_illegal.Contains(start) || _illegal.Contains(end) || start == end)
            return [];

        HashSet<int> done = [start];
        Queue<(int, int[])> todo = new([(start, [])]);

        while (true)
        {
            var cur = todo.Dequeue();
            for (int i = 0; i < 5; i++)
            {
                var next = cur.Item1 ^ (1 << i);
                if (next == end)
                    return [.. cur.Item2, i];
                if (!_illegal.Contains(next) && !done.Contains(next))
                {
                    done.Add(next);
                    todo.Enqueue((next, [.. cur.Item2, i]));
                }
            }
        }
    }

    private static readonly HashSet<int> _illegal = [0b00100, 0b01011, 0b01111, 0b10010, 0b10011, 0b10111, 0b11000, 0b11010, 0b11100, 0b11110];
}
