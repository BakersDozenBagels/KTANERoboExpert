using KTANERoboExpert.Modules.Vanilla;
using KTANERoboExpert.Uncertain;
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Speech.Recognition;
using System.Text.RegularExpressions;

namespace KTANERoboExpert.Modules;

public class Poetry : RoboExpertModule
{
    public override string Name => "Poetry";
    public override string Help => "blue (eye color, jane is purple) clarity ocean ... -> past solitary ...";
    private Grammar? _grammar, _subgrammar;
    public override Grammar Grammar => _girl.Exists ? Subgrammar : Maingrammar;
    private Grammar Maingrammar => _grammar ??= new(new GrammarBuilder(new Choices("blue", "green", "pink", "purple", "melanie", "jane", "hana", "lacy")) + new GrammarBuilder(new Choices([.. _words.Where(w => w is not "")]), 6, 6));
    private Grammar Subgrammar => _subgrammar ??= new(new GrammarBuilder(new Choices([.. _words.Where(w => w is not "")]), 6, 6));

    private Maybe<string> _girl;
    private int _stage;

    public override void ProcessCommand(string command)
    {
        var parts = command.Split(' ');
        if (parts.Length is 7)
        {
            _girl = parts[0];
            parts = parts[1..];
            ExitSubmenu();
            EnterSubmenu(Subgrammar);
        }

        Speak(_words[parts.Select(w => _words.IndexOf(w)).MinBy(i => _girl.Item switch
        {
            "blue" or "melanie" => (i % 6) + (i / 6),
            "green" or "lacy" => -(i % 6) - (i / 6),
            "pink" or "hana" => (i % 6) - (i / 6),
            "purple" or "jane" => -(i % 6) + (i / 6),
            _ => throw new UnreachableException()
        })]);

        _stage++;
        if (_stage is 3)
        {
            Reset();
            ExitSubmenu();
            Solve();
        }
    }

    public override void Reset()
    {
        _girl = default;
        _stage = 0;
    }

    private static readonly string[] _words = [
        "", "clarity", "flow", "fatigue", "hollow", "",
        "energy", "sunshine", "ocean", "reflection", "identity", "black",
        "crowd", "heart", "weather", "words", "past", "solitary",
        "relax", "dance", "weightless", "morality", "gaze", "failure",
        "bunny", "lovely", "romance", "future", "focus", "search",
        "", "cookies", "compassion", "creation", "patience", "",
    ];
}
