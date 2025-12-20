using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class LEDGrid : RoboExpertModule
{
    public override string Name => "LED Grid";
    public override string Help => "red purple black white ...";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices([.. Enum.GetValues<Color>().Select(c => c.ToString())]), 9, 9));

    public override void ProcessCommand(string command)
    {
        var colors = command.Split(' ').Select(Enum.Parse<Color>).ToArray();
        var pairs = Enum.GetValues<Color>().Where(c => colors.Count(d => d == c) is 2).ToArray();
        switch (colors.Count(c => c is Color.Black))
        {
            case 0:
                if (!colors.Any(c => c is Color.Orange))
                    Answer("CDAB");
                else if (colors.Count(c => c is Color.Red) is >= 3)
                    Answer("DACB");
                else if (pairs.Length is >= 2)
                    Answer("BACD");
                else if (colors[6] == colors[7] && colors[7] == colors[8])
                    Answer("ACDB");
                else
                    Answer("BCDA");
                return;

            case 1:
                if (colors.Distinct().Count() is 9)
                    Answer("DCBA");
                else if (colors[0] == colors[1] && colors[1] == colors[2])
                    Answer("ADBC");
                else if (colors.Count(c => c is Color.Red) is 3 || colors.Count(c => c is Color.Pink) is 3 || colors.Count(c => c is Color.Purple) is 3)
                    Answer("CBAD");
                else if (colors.Count(c => c is Color.White) is 1 || colors.Count(c => c is Color.Blue) is 2 || colors.Count(c => c is Color.Yellow) is 3)
                    Answer("BADC");
                else
                    Answer("DBAC");
                return;

            case 2:
                if (colors.Count(c => c is Color.Purple) is >= 3)
                    Answer("ADCB");
                else if (pairs.Length is 2)
                    Answer("BCAD");
                else if (colors.Any(c => c is Color.White) && colors.Any(c => c is Color.Orange) && colors.Any(c => c is Color.Pink))
                    Answer("DBCA");
                else if (colors.Count(c => c is Color.Green) is 1 || colors.Count(c => c is Color.Yellow) is 2 || colors.Count(c => c is Color.Red) is 3 || colors.Count(c => c is Color.Blue) is 4)
                    Answer("CADB");
                else
                    Answer("CDBA");
                return;

            case 3:
                if (colors.Count(c => c is Color.Orange) is 2)
                    Answer("BDAC");
                else if (pairs.Length is > 1)
                    Answer("CABD");
                else if (!colors.Any(c => c is Color.Purple))
                    Answer("DCAB");
                else if (colors.Any(c => c is Color.Red) && colors.Any(c => c is Color.Yellow))
                    Answer("ACBD");
                else
                    Answer("BDCA");
                return;

            case 4:
                if (colors[3] == colors[4] && colors[4] == colors[5])
                    Answer("BCDA");
                else if (colors.Count(c => c is Color.Green) is >= 2)
                    Answer("ABDC");
                else if (pairs.Length is 2)
                    Answer("CBDA");
                else if (!colors.Any(c => c is Color.Pink))
                    Answer("DABC");
                else
                    Answer("ABCD");
                return;

            default:
                Speak("Pardon?");
                return;
        }
    }

    private void Answer(string v)
    {
        Speak(v.Select(c => NATO.ElementAt(c - 'A')).Conjoin());
        ExitSubmenu();
        Solve();
    }

    private enum Color
    {
        Black,
        Red,
        Blue,
        Yellow,
        Green,
        Orange,
        Pink,
        Purple,
        White
    }
}
