﻿using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class WhosOnFirst : RoboExpertModule
{
    public override string Name => "Who's on First";
    public override string Help => "YES then READY then FIRST then NO then BLANK then NOTHING then YES done";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices(_displayPhrases.Keys.ToArray()) + new GrammarBuilder("then" + new Choices(_buttonPhrases.Keys.ToArray()).ToGrammarBuilder(), 6, 6) + "done");
    private int _stagesDone;

    public override void ProcessCommand(string command)
    {
        var parts = command[..^5].Split(" then ");
        parts = [_displayPhrases[parts[0]], .. parts[1..].Select(p => _buttonPhrases[p])];
        Speak("Press " + (Array.IndexOf(parts[1..], _lists[parts[_eyes[parts[0]] + 1]].First(parts[1..].Contains)) + 1));
        _stagesDone++;
        if (_stagesDone == 3)
        {
            _stagesDone = 0;
            ExitSubmenu();
        }
    }

    public override void Select() => Speak("Go on Who's on First stage " + (_stagesDone + 1));

    public override void Reset() => _stagesDone = 0;

    // _displayPhrases and _eyes could in theory be merged into one lookup
    // _buttonPhrases and _lists can't be merged this way
    private static readonly Dictionary<string, string> _displayPhrases = new()
    {
        ["YES"] = "YES",
        ["Y E S"] = "YES",
        ["FIRST"] = "FIRST",
        ["F I R S T"] = "FIRST",
        ["DISPLAY"] = "DISPLAY",
        ["D I S P L A Y"] = "DISPLAY",
        ["OKAY"] = "OKAY",
        ["O K A Y"] = "OKAY",
        ["SAYS"] = "SAYS",
        ["S A Y S"] = "SAYS",
        ["NOTHING"] = "NOTHING",
        ["NOTHING WORD"] = "NOTHING",
        ["WORD NOTHING"] = "NOTHING",
        ["N O T H I N G"] = "NOTHING",
        ["EMPTY"] = "",
        ["LITERALLY EMPTY"] = "",
        ["LITERALLY NOTHING"] = "",
        ["LITERALLY BLANK"] = "",
        ["BLANK"] = "BLANK",
        ["BLANK WORD"] = "BLANK",
        ["WORD BLANK"] = "BLANK",
        ["B L A N K"] = "BLANK",
        ["NO"] = "NO",
        ["N O"] = "NO",
        ["L E D"] = "LED",
        ["L E A D"] = "LEAD",
        ["R E A D"] = "READ",
        ["READ A BOOK"] = "READ",
        ["BOOK READ"] = "READ",
        ["RED COLOR"] = "RED",
        ["COLOR RED"] = "RED",
        ["R E D"] = "RED",
        ["R E E D"] = "REED",
        ["REED GRASS"] = "REED",
        ["GRASS REED"] = "REED",
        ["L E E D"] = "LEED",
        ["HOLD ON"] = "HOLD ON",
        ["H O L D O N"] = "HOLD ON",
        ["Y O U"] = "YOU",
        ["YOU WORD"] = "YOU",
        ["WORD YOU"] = "YOU",
        ["Y O U"] = "YOU",
        ["YOU ARE WORDS"] = "YOU ARE",
        ["Y O U A R E"] = "YOU ARE",
        ["Y O U R"] = "YOUR",
        ["YOUR POSSESIVE"] = "YOUR",
        ["YOU'RE APOSTROPHE"] = "YOU'RE",
        ["YOU ARE APOSTROPHE"] = "YOU'RE",
        ["Y O U R E"] = "YOU'RE",
        ["Y O U APOSTROPHE R E"] = "YOU'RE",
        ["U R LETTERS"] = "UR",
        ["THERE"] = "THERE",
        ["OVER THERE"] = "THERE",
        ["T H E R E"] = "THERE",
        ["T H E Y R E"] = "THEY'RE",
        ["T H E Y APOSTROPHE R E"] = "THEY'RE",
        ["THEY ARE APOSTROPHE"] = "THEY'RE",
        ["THEY'RE APOSTROPHE"] = "THEY'RE",
        ["THEIR POSSESSIVE"] = "THEIR",
        ["T H E I R"] = "THEIR",
        ["THEY ARE"] = "THEY ARE",
        ["THEY ARE WORDS"] = "THEY ARE",
        ["T H E Y A R E"] = "THEY ARE",
        ["S E E"] = "SEE",
        ["LOOK SEE"] = "SEE",
        ["SEE LOOK"] = "SEE",
        ["C LETTER"] = "C",
        ["LETTER C"] = "C",
        ["C E E"] = "CEE",
    };
    private static readonly Dictionary<string, string> _buttonPhrases = new()
    {
        ["READY"] = "READY",
        ["R E A D Y"] = "READY",
        ["FIRST"] = "FIRST",
        ["F I R S T"] = "FIRST",
        ["NO"] = "NO",
        ["N O"] = "NO",
        ["BLANK"] = "BLANK",
        ["B L A N K"] = "BLANK",
        ["NOTHING"] = "NOTHING",
        ["N O T H I N G"] = "NOTHING",
        ["YES"] = "YES",
        ["Y E S"] = "YES",
        ["WHAT"] = "WHAT",
        ["W H A T"] = "WHAT",
        ["U H H H"] = "UHHH",
        ["LEFT"] = "LEFT",
        ["L E F T"] = "LEFT",
        ["RIGHT"] = "RIGHT",
        ["R I G H T"] = "RIGHT",
        ["MIDDLE"] = "MIDDLE",
        ["M I D D L E"] = "MIDDLE",
        ["OKAY"] = "OKAY",
        ["O K A Y"] = "OKAY",
        ["WAIT"] = "WAIT",
        ["W A I T"] = "WAIT",
        ["PRESS"] = "PRESS",
        ["P R E S S"] = "PRESS",
        ["Y O U"] = "YOU",
        ["YOU WORD"] = "YOU",
        ["WORD YOU"] = "YOU",
        ["YOU ARE WORDS"] = "YOU ARE",
        ["Y O U A R E"] = "YOU ARE",
        ["YOUR POSSESSIVE"] = "YOUR",
        ["Y O U R"] = "YOUR",
        ["YOU'RE APOSTROPHE"] = "YOU'RE",
        ["YOU ARE APOSTROPHE"] = "YOU'RE",
        ["Y O U R E"] = "YOU'RE",
        ["Y O U APOSTROPHE R E"] = "YOU'RE",
        ["U R LETTERS"] = "UR",
        ["U LETTER"] = "U",
        ["LETTER U"] = "U",
        ["UH HUH YES"] = "UH HUH",
        ["UH HUH POSITIVE"] = "UH HUH",
        ["U H H U H"] = "UH HUH",
        ["UH UH NO"] = "UH UH",
        ["UH UH NEGATIVE"] = "UH UH",
        ["U H U H"] = "UH UH",
        ["WHAT QUESTION MARK"] = "WHAT?",
        ["W H A T QUESTION MARK"] = "WHAT?",
        ["DONE"] = "DONE",
        ["D O N E"] = "DONE",
        ["NEXT"] = "NEXT",
        ["N E X T"] = "NEXT",
        ["HOLD"] = "HOLD",
        ["H O L D"] = "HOLD",
        ["SURE"] = "SURE",
        ["S U R E"] = "SURE",
        ["LIKE"] = "LIKE",
        ["L I K E"] = "LIKE",
    };

    private static readonly Dictionary<string, int> _eyes = new()
    {
        ["YES"] = 2,
        ["FIRST"] = 1,
        ["DISPLAY"] = 5,
        ["OKAY"] = 1,
        ["SAYS"] = 5,
        ["NOTHING"] = 2,
        [""] = 4,
        ["BLANK"] = 3,
        ["NO"] = 5,
        ["LED"] = 2,
        ["LEAD"] = 5,
        ["READ"] = 3,
        ["RED"] = 3,
        ["REED"] = 4,
        ["LEED"] = 4,
        ["HOLD ON"] = 5,
        ["YOU"] = 3,
        ["YOU ARE"] = 5,
        ["YOUR"] = 3,
        ["YOU'RE"] = 3,
        ["UR"] = 0,
        ["THERE"] = 5,
        ["THEY'RE"] = 4,
        ["THEIR"] = 3,
        ["THEY ARE"] = 2,
        ["SEE"] = 5,
        ["C"] = 1,
        ["CEE"] = 5,
    };
    private static readonly Dictionary<string, string[]> _lists = new()
    {
        ["READY"] = ["YES", "OKAY", "WHAT", "MIDDLE", "LEFT", "PRESS", "RIGHT", "BLANK", "READY", "NO", "FIRST", "UHHH", "NOTHING", "WAIT"],
        ["FIRST"] = ["LEFT", "OKAY", "YES", "MIDDLE", "NO", "RIGHT", "NOTHING", "UHHH", "WAIT", "READY", "BLANK", "WHAT", "PRESS", "FIRST"],
        ["NO"] = ["BLANK", "UHHH", "WAIT", "FIRST", "WHAT", "READY", "RIGHT", "YES", "NOTHING", "LEFT", "PRESS", "OKAY", "NO", "MIDDLE"],
        ["BLANK"] = ["WAIT", "RIGHT", "OKAY", "MIDDLE", "BLANK", "PRESS", "READY", "NOTHING", "NO", "WHAT", "LEFT", "UHHH", "YES", "FIRST"],
        ["NOTHING"] = ["UHHH", "RIGHT", "OKAY", "MIDDLE", "YES", "BLANK", "NO", "PRESS", "LEFT", "WHAT", "WAIT", "FIRST", "NOTHING", "READY"],
        ["YES"] = ["OKAY", "RIGHT", "UHHH", "MIDDLE", "FIRST", "WHAT", "PRESS", "READY", "NOTHING", "YES", "LEFT", "BLANK", "NO", "WAIT"],
        ["WHAT"] = ["UHHH", "WHAT", "LEFT", "NOTHING", "READY", "BLANK", "MIDDLE", "NO", "OKAY", "FIRST", "WAIT", "YES", "PRESS", "RIGHT"],
        ["UHHH"] = ["READY", "NOTHING", "LEFT", "WHAT", "OKAY", "YES", "RIGHT", "NO", "PRESS", "BLANK", "UHHH", "MIDDLE", "WAIT", "FIRST"],
        ["LEFT"] = ["RIGHT", "LEFT", "FIRST", "NO", "MIDDLE", "YES", "BLANK", "WHAT", "UHHH", "WAIT", "PRESS", "READY", "OKAY", "NOTHING"],
        ["RIGHT"] = ["YES", "NOTHING", "READY", "PRESS", "NO", "WAIT", "WHAT", "RIGHT", "MIDDLE", "LEFT", "UHHH", "BLANK", "OKAY", "FIRST"],
        ["MIDDLE"] = ["BLANK", "READY", "OKAY", "WHAT", "NOTHING", "PRESS", "NO", "WAIT", "LEFT", "MIDDLE", "RIGHT", "FIRST", "UHHH", "YES"],
        ["OKAY"] = ["MIDDLE", "NO", "FIRST", "YES", "UHHH", "NOTHING", "WAIT", "OKAY", "LEFT", "READY", "BLANK", "PRESS", "WHAT", "RIGHT"],
        ["WAIT"] = ["UHHH", "NO", "BLANK", "OKAY", "YES", "LEFT", "FIRST", "PRESS", "WHAT", "WAIT", "NOTHING", "READY", "RIGHT", "MIDDLE"],
        ["PRESS"] = ["RIGHT", "MIDDLE", "YES", "READY", "PRESS", "OKAY", "NOTHING", "UHHH", "BLANK", "LEFT", "FIRST", "WHAT", "NO", "WAIT"],
        ["YOU"] = ["SURE", "YOU ARE", "YOUR", "YOU'RE", "NEXT", "UH HUH", "UR", "HOLD", "WHAT?", "YOU", "UH UH", "LIKE", "DONE", "U"],
        ["YOU ARE"] = ["YOUR", "NEXT", "LIKE", "UH HUH", "WHAT?", "DONE", "UH UH", "HOLD", "YOU", "U", "YOU'RE", "SURE", "UR", "YOU ARE"],
        ["YOUR"] = ["UH UH", "YOU ARE", "UH HUH", "YOUR", "NEXT", "UR", "SURE", "U", "YOU'RE", "YOU", "WHAT?", "HOLD", "LIKE", "DONE"],
        ["YOU'RE"] = ["YOU", "YOU'RE", "UR", "NEXT", "UH UH", "YOU ARE", "U", "YOUR", "WHAT?", "UH HUH", "SURE", "DONE", "LIKE", "HOLD"],
        ["UR"] = ["DONE", "U", "UR", "UH HUH", "WHAT?", "SURE", "YOUR", "HOLD", "YOU'RE", "LIKE", "NEXT", "UH UH", "YOU ARE", "YOU"],
        ["U"] = ["UH HUH", "SURE", "NEXT", "WHAT?", "YOU'RE", "UR", "UH UH", "DONE", "U", "YOU", "LIKE", "HOLD", "YOU ARE", "YOUR"],
        ["UH HUH"] = ["UH HUH", "YOUR", "YOU ARE", "YOU", "DONE", "HOLD", "UH UH", "NEXT", "SURE", "LIKE", "YOU'RE", "UR", "U", "WHAT?"],
        ["UH UH"] = ["UR", "U", "YOU ARE", "YOU'RE", "NEXT", "UH UH", "DONE", "YOU", "UH HUH", "LIKE", "YOUR", "SURE", "HOLD", "WHAT?"],
        ["WHAT?"] = ["YOU", "HOLD", "YOU'RE", "YOUR", "U", "DONE", "UH UH", "LIKE", "YOU ARE", "UH HUH", "UR", "NEXT", "WHAT?", "SURE"],
        ["DONE"] = ["SURE", "UH HUH", "NEXT", "WHAT?", "YOUR", "UR", "YOU'RE", "HOLD", "LIKE", "YOU", "U", "YOU ARE", "UH UH", "DONE"],
        ["NEXT"] = ["WHAT?", "UH HUH", "UH UH", "YOUR", "HOLD", "SURE", "NEXT", "LIKE", "DONE", "YOU ARE", "UR", "YOU'RE", "U", "YOU"],
        ["HOLD"] = ["YOU ARE", "U", "DONE", "UH UH", "YOU", "UR", "SURE", "WHAT?", "YOU'RE", "NEXT", "HOLD", "UH HUH", "YOUR", "LIKE"],
        ["SURE"] = ["YOU ARE", "DONE", "LIKE", "YOU'RE", "YOU", "HOLD", "UH HUH", "UR", "SURE", "U", "WHAT?", "NEXT", "YOUR", "UH UH"],
        ["LIKE"] = ["YOU'RE", "NEXT", "U", "UR", "HOLD", "DONE", "UH UH", "WHAT?", "UH HUH", "YOU", "LIKE", "SURE", "YOU ARE", "YOUR"],
    };
}
