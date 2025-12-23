using KTANERoboExpert.Uncertain;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class PurpleArrows : RoboExpertModule
{
    public override string Name => "Purple Arrows";
    public override string Help => "hotel echo ... -> tango -(right)-> insure -(right)-> ...";
    private Grammar? _grammar, _subgrammar;
    public override Grammar Grammar => _word.Exists ? Subgrammar : Maingrammar;
    private Grammar Maingrammar => _grammar ??= new(new GrammarBuilder(new Choices([.. NATO]), 6, 6));
    private Grammar Subgrammar => _subgrammar ??= new(new GrammarBuilder(new Choices([.. NATO])));

    private Maybe<string> _word;
    private readonly List<char> _path = [];

    public override void ProcessCommand(string command)
    {
        var parts = new string([.. command.Split(' ').Select(w => w[0])]);
        if (parts.Length is 6)
        {
            var w = _words.Where(w => parts.Order().SequenceEqual(w.Order())).ToArray();
            if (w.Length is 1)
            {
                _word = w[0];
                ExitSubmenu();
                EnterSubmenu(Subgrammar);
                Speak(w[0]);
            }

            return;
        }

        if (!_word.Exists)
            return;

        _path.Add(parts[0]);
        var ix = FindIndex();
        if (!ix.Exists)
        {
            Speak("Next");
            return;
        }

        var goal = _words.IndexOf(_word.Item);
        int gx = goal % 9, gy = goal / 9,
            sx = ix.Item % 9, sy = ix.Item / 9,
            dx = (gx - sx + 9) % 9, dy = (gy - sy + 11) % 11;

        string cmd = "";
        if (dx > 5)
            cmd = "Left " + (9 - dx);
        else if (dx is not 0)
            cmd = "Right " + dx;

        if (dy > 5)
            cmd += " Up " + (11 - dy);
        else if (dy is not 0)
            cmd += " Down " + dy;

        if (cmd is "")
            Speak("submit");
        else
            Speak(cmd.Trim());
        ExitSubmenu();
        Reset();
        Solve();
    }

    private Maybe<int> FindIndex()
    {
        var m = Enumerable.Range(0, 117).Where(Matches).ToArray();
        while (m.Length is 0)
        {
            _path.RemoveAt(0);
            m = [.. Enumerable.Range(0, 117).Where(Matches)];
        }
        if (m.Length is not 1)
            return default;

        int x = m[0] % 9, y = m[0] / 9;
        return (x + _path.Count - 1) % 9 + 9 * y;
    }

    private bool Matches(int ix)
    {
        int x = ix % 9, y = ix / 9;
        for (int i = 0; i < _path.Count; i++)
            if (_words[(x + i) % 9 + 9 * y][0] != _path[i])
                return false;
        return true;
    }

    public override void Select()
    {
        if (_word.Exists)
            Speak("Go on Purple Arrows, word " + _word.Item);
        else
            base.Select();
    }

    public override void Reset()
    {
        _word = default;
        _path.Clear();
    }

    private static readonly string[] _words =
    [
        "thesis", "immune", "agency", "height", "active", "bother", "viable", "expose", "border",
        "insure", "insist", "behave", "thread", "apathy", "offend", "extend", "vessel", "earwax",
        "occupy", "prince", "pardon", "weight", "harbor", "trench", "absorb", "outfit", "injury",
        "honest", "refuse", "access", "punish", "valley", "writer", "happen", "bucket", "agenda",
        "bubble", "tycoon", "health", "hammer", "useful", "offset", "quaint", "bomber", "detail",
        "result", "energy", "pigeon", "excuse", "please", "relate", "appear", "thanks", "visual",
        "trance", "dinner", "throne", "danker", "wealth", "jacket", "tumble", "weapon", "wonder",
        "bounce", "hiccup", "unique", "prayer", "bronze", "endure", "timber", "inside", "embark",
        "pledge", "poetry", "velvet", "waiter", "estate", "belong", "ignore", "hotdog", "regret",
        "rotten", "adjust", "expand", "borrow", "treaty", "player", "junior", "wander", "helmet",
        "impact", "bottom", "ticket", "gossip", "retire", "infect", "direct", "battle", "divide",
        "virtue", "update", "peanut", "ignite", "quebec", "thrust", "artist", "accept", "random",
        "remedy", "insert", "hunter", "turkey", "winner", "theory", "import", "outlet", "buffet",
    ];
}
