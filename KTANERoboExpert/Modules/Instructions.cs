using KTANERoboExpert.Uncertain;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules;

public partial class Instructions : RoboExpertModule
{
    public override string Name => "Instructions";
    public override string Help => "BATTERIES, BRAVO, ... (5 screens in order) red alfa, ... (4 buttons in order)";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(EdgeworkScreens + ButtonScreens + EdgeworkScreens + ButtonScreens + ButtonScreens + Buttons);
    private static GrammarBuilder EdgeworkScreens => new Choices("BATTERIES", "BATTERY HOLDERS", "INDICATORS", "LIT INDICATORS", "UNLIT INDICATORS", "PORTS", "PORT PLATES", "DIGITS IN SERIAL NUMBER", "LETTERS IN SERIAL NUMBER", "MODULES", "TWO FACTORS", "SOLVED MODULES", "PORT TYPES", "STRIKES", "DATE OF MANUFACTURES", "TIME OF DAY WIDGETS");
    private static GrammarBuilder ButtonScreens => new Choices("RED", "GREEN", "YELLOW", "BLUE", "ALFA", "BRAVO", "CHARLIE", "DELTA", "FIRST", "SECOND", "THIRD", "FOURTH");
    private static GrammarBuilder Buttons => new(new GrammarBuilder(new Choices("RED", "GREEN", "YELLOW", "BLUE")) + new Choices("ALFA", "BRAVO", "CHARLIE", "DELTA"), 4, 4);

    private static readonly string[] _ordinal = ["First", "Second", "Third", "Fourth"];

    public override void ProcessCommand(string command)
    {
        var m = CommandMatcher().Match(command);

        UncertainInt
            s1 = ProcessEdgework(m.Groups[1].Value),
            s3 = ProcessEdgework(m.Groups[3].Value);

        Button[] buttons = [.. m.Groups[6].Captures.Zip(m.Groups[7].Captures, (a, b) => new Button(b.Value[0], a.Value[0]))];

        if (buttons.Select(b => b.Label).Distinct().Count() != 4 || buttons.Select(b => b.Color).Distinct().Count() != 4)
            return;

        int s2 = Find(buttons, m.Groups[2].Value),
            s4 = Find(buttons, m.Groups[4].Value),
            s5 = Find(buttons, m.Groups[5].Value);

        var x = (UncertainCondition<UncertainInt>.Of(s1 == 0, s5 == s2 ? s4 : Math.Max(s5, s2))
            | (s3 > s1, s2)
            | (s2 < s4, s5)
            | (s3 > 3, s1.Map(v => v % 4).Into())
            | (s2 != s4 && s4 != s5 && s5 != s2, ((int[])[0, 1, 2, 3]).First(x => x != s2 && x != s4 && x != s5))
            | s4).FlatMap();

        if (!x.IsCertain)
        {
            x.Fill(() => ProcessCommand(command), ExitSubmenu);
            return;
        }

        Speak(_ordinal[x.Value]);
        ExitSubmenu();
        Solve();
    }

    private static int Find(Button[] buttons, string value)
    {
        if (value is "RED" or "GREEN" or "YELLOW" or "BLUE")
            return buttons.IndexOf(x => x.Color == value[0]);
        if (value is "ALFA" or "BRAVO" or "CHARLIE" or "DELTA")
            return buttons.IndexOf(x => x.Label == value[0]);
        return value switch
        {
            "FIRST" => 0,
            "SECOND" => 1,
            "THIRD" => 2,
            "FOURTH" => 3,
            _ => throw new UnreachableException()
        };
    }

    private static UncertainInt ProcessEdgework(string query) => query switch
    {
        "BATTERIES" => Edgework.Batteries,
        "BATTERY HOLDERS" => Edgework.BatteryHolders,
        "INDICATORS" => Edgework.Indicators.Count,
        "LIT INDICATORS" => Edgework.Indicators.Where(i => i.Lit).Count,
        "UNLIT INDICATORS" => Edgework.Indicators.Where(i => !i.Lit).Count,
        "PORTS" => Edgework.Ports.Count,
        "PORT PLATES" => Edgework.PortPlates.Count,
        "DIGITS IN SERIAL NUMBER" => Edgework.SerialNumberDigits().Count,
        "LETTERS IN SERIAL NUMBER" => Edgework.SerialNumberLetters().Count,
        "MODULES" => Edgework.TotalModuleCount,
        "TWO FACTORS" => Edgework.TwoFactorCount,
        "SOLVED MODULES" => Edgework.Solves,
        "PORT TYPES" => Edgework.PortTypes.Count,
        "STRIKES" => Edgework.Strikes,
        "DATE OF MANUFACTURES" => Edgework.DateOfManufactureWidgets,
        "TIME OF DAY WIDGETS" => Edgework.TimeOfDayWidgets,
        _ => throw new UnreachableException()
    };

    private record struct Button(char Label, char Color);

    [GeneratedRegex("^(BATTERIES|BATTERY HOLDERS|INDICATORS|LIT INDICATORS|UNLIT INDICATORS|PORTS|PORT PLATES|DIGITS IN SERIAL NUMBER|LETTERS IN SERIAL NUMBER|MODULES|TWO FACTORS|SOLVED MODULES|PORT TYPES|STRIKES|DATE OF MANUFACTURES|TIME OF DAY WIDGETS) (RED|GREEN|YELLOW|BLUE|ALFA|BRAVO|CHARLIE|DELTA|FIRST|SECOND|THIRD|FOURTH) (BATTERIES|BATTERY HOLDERS|INDICATORS|LIT INDICATORS|UNLIT INDICATORS|PORTS|PORT PLATES|DIGITS IN SERIAL NUMBER|LETTERS IN SERIAL NUMBER|MODULES|TWO FACTORS|SOLVED MODULES|PORT TYPES|STRIKES|DATE OF MANUFACTURES|TIME OF DAY WIDGETS) (RED|GREEN|YELLOW|BLUE|ALFA|BRAVO|CHARLIE|DELTA|FIRST|SECOND|THIRD|FOURTH) (RED|GREEN|YELLOW|BLUE|ALFA|BRAVO|CHARLIE|DELTA|FIRST|SECOND|THIRD|FOURTH) (?:(RED|GREEN|YELLOW|BLUE) (ALFA|BRAVO|CHARLIE|DELTA) ?){4}$")]
    private static partial Regex CommandMatcher();
}
