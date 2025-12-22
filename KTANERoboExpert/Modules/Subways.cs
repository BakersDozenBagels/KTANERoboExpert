using KTANERoboExpert.Uncertain;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules;

public partial class Subways : RoboExpertModule
{
    public override string Name => "Subways";
    public override string Help => "London Mary Tuesday";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("London", "New York", "Paris")) + new Choices("Bryan", "John", "Mike", "Emily", "Mary", "Katie") + new Choices("Monday", "Tuesday", "Wednesday", "Thursday", "Friday"));

    public override void ProcessCommand(string command)
    {
        var m = CommandMatcher().Match(command);
        var sol = _table[m.Groups[1].Value][m.Groups[2].Value][m.Groups[3].Value switch
        {
            "Monday" => 0,
            "Tuesday" => 1,
            "Wednesday" => 2,
            "Thursday" => 3,
            "Friday" => 4,
            _ => throw new UnreachableException()
        }];

        var time = sol.time.Map(UncertainInt.Exactly).OrElse(Edgework.Batteries);
        if (!time.IsCertain)
        {
            time.Fill(() => ProcessCommand(command), ExitSubmenu);
            return;
        }

        Speak(_times[time.Value] + " " + _routes[sol.route]);
        ExitSubmenu();
        Solve();
    }

    private static readonly Dictionary<string, Dictionary<string, (int route, Maybe<int> time)[]>> _table = new()
    {
        ["New York"] = new()
        {
            ["Bryan"] = [(1, 8), (8, 19), (4, 4), (3, 11), (6, 12)],
            ["John"] = [(6, 7), (1, 2), (2, 13), (7, default), (3, 16)],
            ["Mike"] = [(7, default), (2, 3), (5, 18), (8, 9), (4, default)],
            ["Emily"] = [(8, 20), (2, 1), (1, 14), (3, default), (5, 23)],
            ["Mary"] = [(7, 6), (1, default), (4, 15), (6, 5), (2, 17)],
            ["Katie"] = [(5, 0), (7, 22), (3, default), (8, 10), (4, 21)],
        },
        ["London"] = new()
        {
            ["Bryan"] = [(9, 1), (14, default), (13, 17), (10, 5), (15, 18)],
            ["John"] = [(13, default), (11, 12), (10, 2), (16, 4), (14, 9)],
            ["Mike"] = [(9, 8), (16, 19), (12, default), (11, 21), (15, 23)],
            ["Emily"] = [(13, 11), (9, 16), (10, 3), (16, 13), (12, default)],
            ["Mary"] = [(13, 7), (16, 14), (11, 0), (9, default), (12, 10)],
            ["Katie"] = [(12, default), (14, 20), (9, 6), (13, 3), (16, 22)],
        },
        ["Paris"] = new()
        {
            ["Bryan"] = [(17, default), (18, 9), (21, 20), (22, 14), (19, 7)],
            ["John"] = [(20, 3), (19, 22), (23, default), (18, 10), (22, 12)],
            ["Mike"] = [(20, 17), (21, default), (23, 11), (18, 8), (24, 4)],
            ["Emily"] = [(17, 12), (22, 13), (24, 21), (18, 18), (20, default)],
            ["Mary"] = [(19, 5), (21, 15), (23, 6), (24, default), (17, 23)],
            ["Katie"] = [(19, 2), (17, default), (20, 19), (21, 1), (23, 16)],
        },
    };

    private static readonly string[] _routes = [
        string.Empty,
        "Canal St 1, then Franklin St 1, then Chambers St 1-2-3",
        "Franklin St 1, then Rector St 1, then South Ferry 1",
        "Canal St J-N-Q-R, then City Hall R-W, then Rector St R-W",
        "South Ferry R-W, then Cortlandt St R-W, then Canal St J-N-Q-R",
        "Chambers St J-Z, then Fulton St, then Broad St J-Z",
        "Wall St 2-3, then Park Place 2-3, then Chambers St 1-2-3",
        "World Trade Center E, then Canal St A-C-E, then Chambers St A-C",
        "Bowling Green 4-5, then Wall St 4-5, then City Hall 4-5-6",
        "Green Park, then Piccadilly Circus, then Leicester Square",
        "Holborn, then Leicester Square, then Green Park",
        "Oxford Circus, then Tottenham Court Road, then Holborn",
        "Warren Street, then Tottenham Court Road, then Leicester Square",
        "Oxford Circus, then Warren Street, then King’s Cross St.Pancras",
        "Warren Street, then Oxford Circus, then Green Park",
        "Holborn, then Piccadilly Circus, then Green Park",
        "King’s Cross St.Pancras, then Warren Street, then Green Park",
        "Richelieu Drouot, then Grands Boulevards, then Bonne Nouvelle",
        "Réaumur Sébastopol, then Sentier, then Bourse",
        "St-Michel, then Cité, then Réaumur Sébastopol",
        "Pont Neuf, then Pont Marie, then Sully Morland",
        "Bonne Nouvelle, then Grands Boulevards, then Richelieu Drouot",
        "Bourse, then Sentier, then Réaumur Sébastopol",
        "Réaumur Sébastopol, then Cité, then St-Michel",
        "Sully Morland, then Pont Marie, then Pont Neuf",
    ];

    private static readonly string[] _times = ["12AM", "1AM", "2AM", "3AM", "4AM", "5AM", "6AM", "7AM", "8AM", "9AM", "10AM", "11AM", "12PM", "1PM", "2PM", "3PM", "4PM", "5PM", "6PM", "7PM", "8PM", "9PM", "10PM", "11PM",];

    [GeneratedRegex("^(London|New York|Paris) (Bryan|John|Mike|Emily|Mary|Katie) (Monday|Tuesday|Wednesday|Thursday|Friday)$")]
    private static partial Regex CommandMatcher();
}
