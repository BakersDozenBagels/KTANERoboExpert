using KTANERoboExpert.Uncertain;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class NnMs : RoboExpertModule
{
    public override string Name => "Novembers and Mikes";
    public override string Help => "mike november mike mike mike ...";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("November", "Mike"), 25, 25));

    public override void ProcessCommand(string command)
    {
        var labels = command.Split(' ').Select(w => w[0]).Chunk(5).Select(c => new string(c)).ToArray();

        if (labels.Distinct().Count() is not 5)
            return;

        bool row = false;
        int ix = -1;
        for (int i = 0; i < 5; i++)
        {
            if (labels.Count(l => InRow(l, i)) is 4)
            {
                if (ix is not -1)
                {
                    Speak("Pardon?");
                    return;
                }
                row = true;
                ix = i;
            }
            if (labels.Count(l => InColumn(l, i)) is 4)
            {
                if (ix is not -1)
                {
                    Speak("Pardon?");
                    return;
                }
                row = false;
                ix = i;
            }
        }

        if (ix is -1)
        {
            Speak("Pardon?");
            return;
        }

        Speak(((string[])["first", "second", "third", "fourth", "fifth"])[labels.IndexOf(l => row ? !InRow(l, ix) : !InColumn(l, ix))]);
        ExitSubmenu();
        Solve();
    }

    private static bool InRow(string l, int r) => _table.AsSpan()[(5 * r)..(5 * r + 5)].Contains(l);
    private static bool InColumn(string l, int c) => Enumerable.Range(0, 5).Any(r => _table[5 * r + c] == l);

    private static readonly string[] _table = [
        "NNNMM", "MNMNN", "NNNNN", "MMNNN", "NMMNM",
        "MMMNM", "MNMNM", "NMNNN", "NNMNN", "MNMMM",
        "MNMMN", "MNNNN", "NMMMM", "NMNMM", "MNNNM",
        "NMNMN", "NNNNM", "MMNMM", "NMMMN", "NNMMN",
        "MMMMM", "NMNNM", "NNNMN", "NNMNM", "MMMMN",
    ];
}
