using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class CrazyTalk : RoboExpertModule
{
    public override string Name => "Crazy Talk";
    public override string Help => "quote [the phrase, no symbols except for arrows (as 'left arrow') or dot dot forms] unquote -> (pick an option if prompted)";
    private Grammar? _grammar, _subgrammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder("quote") + new Choices(_phrases.Keys.Where(k => k[0] != '!').ToArray()) + new GrammarBuilder("unquote"));
    private Grammar Subgrammar => _subgrammar ??= new(new Choices("yes", "no"));

    private Maybe<string> _original;
    private Maybe<string> _group;
    private int _treePosition;
    private bool _inYesNo;
    private Maybe<bool> _yesNo;

    public override void ProcessCommand(string command)
    {
        if (_inYesNo)
        {
            _yesNo = command == "yes";
            ExitSubmenu();
            Tree();
            return;
        }

        _original = command[6..^8];
        var (_, _, group, _) = _phrases[_original.Item!];
        _group = group is null ? default : new(group);
        if (_group.Exists)
            Tree();
        else
            Answer(_original.Item!);
    }

    private void Tree()
    {
        switch (_group.Item!)
        {
            case "l":
                // "←"
                // "LEFT ARROW"

                if (!_yesNo.Exists)
                {
                    YesNo("Is that a symbol?");
                    return;
                }

                Answer(_yesNo.Item ? "left arrow" : "LEFT ARROW");
                return;

            case "1324":
                // "ONE THREE TWO FOUR"
                // "ONE THREE TO FOR"
                // "ONE 3 2 FOUR"
                // "ONE 3 2 4"
                // "1 THREE TWO FOUR"
                // "1 3 2 FOR"
                // "1 3 TO 4"
                // "1 3 TOO 4"
                // "1 3 2 4"

                if (!_yesNo.Exists)
                {
                    YesNo("Is the one a digit?");
                    return;
                }

                if (_treePosition is 0)
                {
                    if (!_yesNo.Item)
                    {
                        _treePosition = 1;
                        YesNo("Is the three a digit?");
                    }
                    else
                    {
                        _treePosition = 2;
                        YesNo("Is the four a digit?");
                    }
                    return;
                }

                if (_treePosition is 1)
                {
                    if (!_yesNo.Item)
                    {
                        _treePosition = 3;
                        YesNo("Is the two spelled correctly?");
                    }
                    else
                    {
                        _treePosition = 4;
                        YesNo("Is the four a digit?");
                    }
                    return;
                }

                if (_treePosition is 3)
                {
                    Answer(_yesNo.Item ? "ONE THREE TWO FOUR" : "ONE THREE TO FOR");
                    return;
                }

                if (_treePosition is 4)
                {
                    Answer(!_yesNo.Item ? "ONE 3 2 FOUR" : "ONE 3 2 4");
                    return;
                }

                if (_treePosition is 2)
                {
                    if (!_yesNo.Item)
                    {
                        _treePosition = 5;
                        YesNo("Is the three a digit?");
                    }
                    else
                    {
                        _treePosition = 6;
                        YesNo("Is the two a digit?");
                    }
                    return;
                }

                if (_treePosition is 5)
                {
                    Answer(!_yesNo.Item ? "1 THREE TWO FOUR" : "1 3 2 FOR");
                    return;
                }

                if (_treePosition is 6)
                {
                    if (_yesNo.Item)
                        Answer("1 3 2 4");
                    else
                    {
                        _treePosition = 7;
                        YesNo("Is the two spelled correctly?");
                    }
                    return;
                }

                if (_treePosition is 7)
                {
                    Answer(!_yesNo.Item ? "1 3 TO 4" : "1 3 TOO 4");
                    return;
                }

                throw new UnreachableException();

            case "1322os4":
                // "1 3 TOO WITH 2 OHS FOUR"
                // "1 3 TOO WITH TWO OS 4"
                // "ONE THREE 2 WITH TWO OHS 4"

                if (!_yesNo.Exists)
                {
                    YesNo("Is the four a word?");
                    return;
                }

                if (_treePosition is 0)
                {

                    if (_yesNo.Item)
                        Answer("1 3 TOO WITH 2 OHS FOUR");
                    else
                    {
                        _treePosition = 1;
                        YesNo("Is the one a word?");
                    }
                    return;
                }

                if (_treePosition is 1)
                {
                    if (_yesNo.Item)
                        Answer("ONE THREE 2 WITH TWO OHS 4");
                    else
                        Answer("1 3 TOO WITH TWO OS 4");
                    return;
                }

                throw new UnreachableException();

            case "..":
                // "STOP."
                // ".STOP"
                // ".."
                // ".PERIOD"
                // "PERIOD"
                // "PERIOD PERIOD"
                // "STOP STOP"
                // "DOT DOT"
                // "FULLSTOP FULLSTOP"

                if (!_yesNo.Exists)
                {
                    YesNo("Any symbols?");
                    return;
                }

                if (_treePosition is 0)
                {
                    _treePosition = 1;

                    if (!_yesNo.Item)
                        Answer(_original.Item!);
                    else
                        YesNo("Literal word stop somewhere?");

                    return;
                }

                if (_treePosition is 1)
                {
                    if (_yesNo.Item)
                    {
                        _treePosition = 2;
                        YesNo("Symbol first?");
                    }
                    else
                    {
                        _treePosition = 3;
                        YesNo("Two symbols?");
                    }
                    return;
                }

                if (_treePosition is 2)
                {
                    Answer(_yesNo.Item ? "!.STOP" : "!STOP.");
                    return;
                }

                if (_treePosition is 3)
                {
                    Answer(_yesNo.Item ? "!.." : "!.PERIOD");
                    return;
                }

                throw new UnreachableException();
        }

        throw new UnreachableException();
    }

    private void YesNo(string ask)
    {
        _inYesNo = true;
        Speak(ask);
        EnterSubmenu(Subgrammar);
    }

    private void Answer(string key)
    {
        var (down, up, _, _) = _phrases[key];
        Speak("down on " + down + ", up on " + up);
        ExitSubmenu();
        Reset();
        Solve();
    }

    public override void Cancel()
    {
        if (_inYesNo)
            ExitSubmenu();
        Reset();
    }

    public override void Reset()
    {
        _inYesNo = default;
        _original = default;
        _group = default;
        _treePosition = default;
        _yesNo = default;
    }

    private static readonly Dictionary<string, (int down, int up, string? group, string original)> _phrases = new()
    {
        ["left arrow left arrow right arrow left arrow right arrow right arrow"] = (5, 4, null, "← ← → ← → →"),
        ["1 3 2 4"] = (3, 2, "1324", "1 3 2 4"),
        ["LEFT ARROW LEFT WORD RIGHT ARROW LEFT WORD RIGHT ARROW RIGHT WORD"] = (5, 8, null, "LEFT ARROW LEFT WORD RIGHT ARROW LEFT WORD RIGHT ARROW RIGHT WORD"),
        ["BLANK"] = (1, 3, null, "BLANK"),
        ["LITERALLY BLANK"] = (1, 5, null, "LITERALLY BLANK"),
        ["FOR THE LOVE OF ALL THAT IS GOOD AND HOLY PLEASE FULLSTOP FULLSTOP"] = (9, 0, null, "FOR THE LOVE OF ALL THAT IS GOOD AND HOLY PLEASE FULLSTOP FULLSTOP."),
        ["AN ACTUAL LEFT ARROW LITERAL PHRASE"] = (5, 3, null, "AN ACTUAL LEFT ARROW LITERAL PHRASE"),
        ["FOR THE LOVE OF THE DISPLAY JUST CHANGED I DIDN'T KNOW THIS MOD COULD DO THAT DOES IT MENTION THAT IN THE MANUAL"] = (8, 7, null, "FOR THE LOVE OF - THE DISPLAY JUST CHANGED, I DIDN’T KNOW THIS MOD COULD DO THAT. DOES IT MENTION THAT IN THE MANUAL?"),
        ["ALL WORDS ONE THREE TO FOR FOR AS IN THIS IS FOR YOU"] = (4, 0, null, "ALL WORDS ONE THREE TO FOR FOR AS IN THIS IS FOR YOU"),
        ["LITERALLY NOTHING"] = (1, 4, null, "LITERALLY NOTHING"),
        ["NO LITERALLY NOTHING"] = (2, 5, null, "NO, LITERALLY NOTHING"),
        ["THE WORD LEFT"] = (7, 0, null, "THE WORD LEFT"),
        ["HOLD ON IT'S BLANK"] = (1, 9, null, "HOLD ON IT’S BLANK"),
        ["SEVEN WORDS FIVE WORDS THREE WORDS THE PUNCTUATION FULLSTOP"] = (0, 5, null, "SEVEN WORDS FIVE WORDS THREE WORDS THE PUNCTUATION FULLSTOP"),
        ["THE PHRASE THE WORD STOP TWICE"] = (9, 1, null, "THE PHRASE THE WORD STOP TWICE"),
        ["THE FOLLOWING SENTENCE THE WORD NOTHING "] = (2, 7, null, "THE FOLLOWING SENTENCE THE WORD NOTHING"),
        ["ONE THREE TO FOR"] = (3, 9, "1324", "ONE THREE TO FOR"),
        ["THREE WORDS THE WORD STOP"] = (7, 3, null, "THREE WORDS THE WORD STOP"),
        ["DISREGARD WHAT I JUST SAID FOUR WORDS NO PUNCTUATION ONE THREE 2 4"] = (3, 1, null, "DISREGARD WHAT I JUST SAID. FOUR WORDS, NO PUNCTUATION. ONE THREE 2 4."),
        ["1 3 2 FOR"] = (1, 0, "1324", "1 3 2 FOR"),
        ["DISREGARD WHAT I JUST SAID TWO WORDS THEN TWO DIGITS ONE THREE 2 4"] = (0, 8, null, "DISREGARD WHAT I JUST SAID. TWO WORDS THEN TWO DIGITS. ONE THREE 2 4."),
        ["WE JUST BLEW UP"] = (4, 2, null, "WE JUST BLEW UP"),
        ["NO REALLY"] = (5, 2, null, "NO REALLY."),
        ["left arrow left right arrow left right arrow right"] = (5, 6, null, "← LEFT → LEFT → RIGHT"),
        ["ONE AND THEN 3 TO 4"] = (4, 7, null, "ONE AND THEN 3 TO 4"),
        ["STOP TWICE"] = (7, 6, null, "STOP TWICE"),
        ["LEFT"] = (6, 9, null, "LEFT"),
        ["!.."] = (8, 5, "..", ".."),
        ["PERIOD PERIOD"] = (8, 2, "..", "PERIOD PERIOD"),
        ["THERE ARE THREE WORDS NO PUNCTUATION READY STOP DOT PERIOD"] = (5, 0, null, "THERE ARE THREE WORDS NO PUNCTUATION READY? STOP DOT PERIOD"),
        ["NOVEBMER OSCAR SPACE LIMA INDIGO TANGO ECHO ROMEO ALPHA LIMA LIMA YANKEE SPACE NOVEMBER OSCAR TANGO HOTEL INDEGO NOVEMBER GOLF"] = (2, 9, null, "NOVEBMER OSCAR SPACE, LIMA INDIGO TANGO ECHO ROMEO ALPHA LIMA LIMA YANKEE SPACE NOVEMBER OSCAR TANGO HOTEL INDEGO NOVEMBER GOLF"),
        ["FIVE WORDS THREE WORDS THE PUNCTUATION FULLSTOP"] = (1, 9, null, "FIVE WORDS THREE WORDS THE PUNCTUATION FULLSTOP"),
        ["THE PHRASE THE PUNCTUATION FULLSTOP"] = (1, 9, null, "THE PHRASE: THE PUNCTUATION FULLSTOP"),
        ["EMPTY SPACE"] = (1, 6, null, "EMPTY SPACE"),
        ["ONE THREE TWO FOUR"] = (3, 7, "1324", "ONE THREE TWO FOUR"),
        ["IT'S SHOWING NOTHING"] = (2, 3, null, "IT’S SHOWING NOTHING"),
        ["LIMA ECHO FOXTROT TANGO SPACE ALPHA ROMEO ROMEO OSCAR RISKY SPACE SIERRA YANKEE MIKE BRAVO OSCAR LIMA"] = (1, 2, null, "LIMA ECHO FOXTROT TANGO SPACE ALPHA ROMEO ROMEO OSCAR RISKY SPACE SIERRA YANKEE MIKE BRAVO OSCAR LIMA"),
        ["ONE 3 2 4"] = (3, 4, "1324", "ONE 3 2 4"),
        ["!STOP."] = (7, 4, "..", "STOP."),
        ["!.PERIOD"] = (8, 1, "..", ".PERIOD"),
        ["NO REALLY STOP"] = (5, 1, null, "NO REALLY STOP"),
        ["1 3 TOO 4"] = (2, 0, "1324", "1 3 TOO 4"),
        ["PERIOD TWICE"] = (8, 3, null, "PERIOD TWICE"),
        ["1 3 TOO WITH 2 OHS FOUR"] = (4, 2, "1322os4", "1 3 TOO WITH 2 OHS FOUR"),
        ["1 3 TO 4"] = (3, 0, "1324", "1 3 TO 4"),
        ["STOP DOT PERIOD"] = (5, 0, null, "STOP DOT PERIOD"),
        ["LEFT LEFT RIGHT LEFT RIGHT RIGHT"] = (6, 7, null, "LEFT LEFT RIGHT LEFT RIGHT RIGHT"),
        ["IT LITERALLY SAYS THE WORD ONE AND THEN THE NUMBERS 2 3 4"] = (4, 5, null, "IT LITERALLY SAYS THE WORD ONE AND THEN THE NUMBERS 2 3 4"),
        ["ONE IN LETTERS 3 2 4 IN NUMBERS"] = (3, 5, null, "ONE IN LETTERS 3 2 4 IN NUMBERS"),
        ["WAIT FORGET EVERYTHING I JUST SAID TWO WORDS THEN TWO SYMBOLS THEN TWO WORDS left arrow left arrow RIGHT LEFT right arrow right arrow"] = (1, 6, null, "WAIT FORGET EVERYTHING I JUST SAID, TWO WORDS THEN TWO SYMBOLS THEN TWO WORDS: ← ← RIGHT LEFT → →"),
        ["1 THREE TWO FOUR"] = (3, 6, "1324", "1 THREE TWO FOUR"),
        ["PERIOD"] = (7, 9, "..", "PERIOD"),
        ["!.STOP"] = (7, 8, "..", ".STOP"),
        ["NOVEBMER OSCAR SPACE, LIMA INDIA TANGO ECHO ROMEO ALPHA LIMA LIMA YANKEE SPACE NOVEMBER OSCAR TANGO HOTEL INDIA NOVEMBER GOLF"] = (0, 7, null, "NOVEBMER OSCAR SPACE, LIMA INDIA TANGO ECHO ROMEO ALPHA LIMA LIMA YANKEE SPACE NOVEMBER OSCAR TANGO HOTEL INDIA NOVEMBER GOLF"),
        ["LIMA ECHO FOXTROT TANGO SPACE ALPHA ROMEO ROMEO OSCAR WHISKEY SPACE SIERRA YANKEE MIKE BRAVO OSCAR LIMA"] = (6, 5, null, "LIMA ECHO FOXTROT TANGO SPACE ALPHA ROMEO ROMEO OSCAR WHISKEY SPACE SIERRA YANKEE MIKE BRAVO OSCAR LIMA"),
        ["NOTHING"] = (1, 2, null, "NOTHING"),
        ["THERE’S NOTHING"] = (1, 8, null, "THERE’S NOTHING"),
        ["STOP STOP"] = (7, 5, "..", "STOP STOP"),
        ["RIGHT ALL IN WORDS STARTING NOW ONE TWO THREE FOUR"] = (4, 9, null, "RIGHT ALL IN WORDS STARTING NOW ONE TWO THREE FOUR"),
        ["THE PHRASE THE WORD LEFT"] = (7, 1, null, "THE PHRASE THE WORD LEFT"),
        ["LEFT ARROW SYMBOL TWICE THEN THE WORDS RIGHT LEFT RIGHT THEN A RIGHT ARROW SYMBOL"] = (5, 9, null, "LEFT ARROW SYMBOL TWICE THEN THE WORDS RIGHT LEFT RIGHT THEN A RIGHT ARROW SYMBOL"),
        ["LEFT LEFT RIGHT left arrow RIGHT right arrow"] = (5, 7, null, "LEFT LEFT RIGHT ← RIGHT →"),
        ["NO COMMA LITERALLY NOTHING"] = (2, 4, null, "NO COMMA LITERALLY NOTHING"),
        ["HOLD ON CRAZY TALK WHILE I DO THIS NEEDY"] = (2, 1, null, "HOLD ON CRAZY TALK WHILE I DO THIS NEEDY"),
        ["THIS ONE IS ALL ARROW SYMBOLS NO WORDS"] = (2, 8, null, "THIS ONE IS ALL ARROW SYMBOLS NO WORDS"),
        ["left arrow"] = (6, 3, "l", "←"),
        ["THE WORD STOP TWICE"] = (9, 4, null, "THE WORD STOP TWICE"),
        ["left arrow left arrow right left right arrow right arrow"] = (6, 1, null, "← ← RIGHT LEFT → →"),
        ["THE PUNCTUATION FULLSTOP"] = (9, 2, null, "THE PUNCTUATION FULLSTOP"),
        ["1 3 TOO WITH TWO OS 4"] = (4, 1, "1322os4", "1 3 TOO WITH TWO OS 4"),
        ["THREE WORDS THE PUNCTUATION FULLSTOP"] = (9, 9, null, "THREE WORDS THE PUNCTUATION FULLSTOP"),
        ["OK WORD FOR WORD LEFT ARROW SYMBOL TWICE THEN THE WORDS RIGHT LEFT RIGHT THEN A RIGHT ARROW SYMBOL"] = (6, 0, null, "OK WORD FOR WORD LEFT ARROW SYMBOL TWICE THEN THE WORDS RIGHT LEFT RIGHT THEN A RIGHT ARROW SYMBOL"),
        ["DOT DOT"] = (8, 6, "..", "DOT DOT"),
        ["LEFT ARROW"] = (6, 8, "l", "LEFT ARROW"),
        ["AFTER I SAY BEEP FIND THIS PHRASE WORD FOR WORD BEEP AN ACTUAL LEFT ARROW"] = (7, 2, null, "AFTER I SAY BEEP FIND THIS PHRASE WORD FOR WORD BEEP AN ACTUAL LEFT ARROW"),
        ["ONE THREE 2 WITH TWO OHS 4"] = (4, 3, "1322os4", "ONE THREE 2 WITH TWO OHS 4"),
        ["LEFT ARROW SYMBOL"] = (6, 4, null, "LEFT ARROW SYMBOL"),
        ["AN ACTUAL LEFT ARROW"] = (6, 2, null, "AN ACTUAL LEFT ARROW"),
        ["THAT'S WHAT IT'S SHOWING"] = (2, 1, null, "THAT’S WHAT IT’S SHOWING"),
        ["THE PHRASE THE WORD NOTHING"] = (2, 6, null, "THE PHRASE THE WORD NOTHING"),
        ["THE WORD ONE AND THEN THE NUMBERS 3 2 4"] = (4, 8, null, "THE WORD ONE AND THEN THE NUMBERS 3 2 4"),
        ["ONE 3 2 FOUR"] = (3, 8, "1324", "ONE 3 2 FOUR"),
        ["ONE WORD THEN PUNCTUATION STOP STOP"] = (0, 9, null, "ONE WORD THEN PUNCTUATION. STOP STOP."),
        ["THE WORD BLANK"] = (0, 1, null, "THE WORD BLANK"),
        ["FULLSTOP FULLSTOP"] = (8, 4, "..", "FULLSTOP FULLSTOP"),
    };
}
