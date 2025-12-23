using System.Diagnostics;
using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules;

public class Battleship : RoboExpertModule
{
    public override string Name => "Battleship";
    public override string Help => "";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder("unused"));

    public override void ProcessCommand(string command) => throw new UnreachableException();

    public override void Select()
    {
        if (!Edgework.SerialNumber.IsCertain)
        {
            Edgework.SerialNumber.Fill(Select, ExitSubmenu);
            return;
        }

        var l = Edgework.SerialNumberLetters().Value!.Zip(Edgework.SerialNumberDigits().Value!);

        Speak("Radar " + l.Distinct().Select(Coord).Conjoin());

        Last();
    }

    private static string Coord((char l, int i) t) => NATO.ElementAt((t.l - 'F') % 5) + " " + ((t.i + 4) % 5 + 1);

    private void Last()
    {
        var l = Edgework.Ports.Count.FlatMap(x => (Edgework.Indicators.Count + Edgework.Batteries).Map(y => ((char)(x + 'A' - 1), y)));

        if (!l.IsCertain)
        {
            l.Fill(Last, ExitSubmenu);
            return;
        }

        Speak("Radar " + Coord(l.Value));

        ExitSubmenu();
        Solve();
    }
}
