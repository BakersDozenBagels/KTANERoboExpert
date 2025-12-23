using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules;

public class WordSearch : RoboExpertModule
{
    public override string Name => "Word Search";
    public override string Help => "Victor Alfa Yankee Whiskey";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices([.. NATO]), 4, 4));

    public override void ProcessCommand(string command)
    {
        var letters = command.Split(' ').Select(s => s.ToUpperInvariant()[0]).ToArray();

        var serial = Edgework.SerialNumberDigits()[^1].Value % 2 == 1 ? 1 : 0;
        string[] words = [
            _chartWords[_chartLetters.IndexOf(letters[0]) + 8][serial],
            _chartWords[_chartLetters.IndexOf(letters[1]) + 7][serial],
            _chartWords[_chartLetters.IndexOf(letters[2]) + 1][serial],
            _chartWords[_chartLetters.IndexOf(letters[3])][serial]
        ];

        Speak(words.Distinct().Conjoin(", ", ", or "));
        ExitSubmenu();
        Solve();
    }

    public override void Select()
    {
        if (!Edgework.SerialNumber.IsCertain)
        {
            Edgework.SerialNumber.Fill(Select, ExitSubmenu);
            return;
        }

        base.Select();
    }

    private static readonly string[][] _chartWords =
        [.. "/;HOTEL/DONE;SEARCH/QUEBEC;ADD/CHECK;SIERRA/FIND;FINISH/EAST;/;PORT/COLOR;BOOM/SUBMIT;LINE/BLUE;KABOOM/ECHO;PANIC/FALSE;MANUAL/ALARM;DECOY/CALL;SEE/TWENTY;INDIA/NORTH;NUMBER/LOOK;ZULU/GREEN;VICTOR/XRAY;DELTA/YES;HELP/LOCATE;ROMEO/BEEP;TRUE/EXPERT;MIKE/EDGE;FOUND/RED;BOMBS/WORD;WORK/UNIQUE;TEST/JINX;GOLF/LETTER;TALK/SIX;BRAVO/SERIAL;SEVEN/TIMER;MODULE/SPELL;LIST/TANGO;YANKEE/SOLVE;/;CHART/OSCAR;MATH/NEXT;READ/LISTEN;LIMA/FOUR;COUNT/OFFICE;/"
            .Split(';')
            .Select(pairStr => pairStr.Split('/'))];

    private static readonly string _chartLetters = ".VUSZ..PQNXFY.TIMEDA.KBWHJO..RLCG..";
}
