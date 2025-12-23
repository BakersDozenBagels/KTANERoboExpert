using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class EuropeanTravel : RoboExpertModule
{
    public override string Name => "European Travel";
    public override string Help => "cyan alfa bravo two three charlie eight";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(_colors)) + new GrammarBuilder(new Choices([.. NATO.Except(["oscar"]), .. Enumerable.Range(0, 10).Select(i => i.ToString())]), 6, 6));

    private static readonly string[] _colors = ["Orange", "Green", "Red", "Cyan", "Yellow", "Pink"];

    public override void ProcessCommand(string command)
    {
        var pieces = command.Split(' ');
        var sn = pieces[1..].Select(s => s[0]).ToArray();

        bool fc = sn[2] switch
        {
            >= 'a' and <= 'z' => true,
            >= '0' and <= '9' => false,
            _ => throw new UnreachableException()
        };
        Speak("Departing " + Depart(pieces[0], sn[0]));
        Speak(sn[4] switch
        {
            >= 'a' and <= 'z' => "Single",
            >= '0' and <= '9' => "Return",
            _ => throw new UnreachableException()
        });
        Speak("Destination " + Destination(pieces[0], sn[1]));
        Speak(fc ? "First Class" : "Second Class");
        Speak("Seat " + sn[5] switch
        {
            >= 'a' and <= 'g' => "1A",
            >= 'h' and <= 'p' => "1B",
            >= 'q' and <= 't' => "2A",
            >= 'u' and <= 'z' => "2B",
            >= '0' and <= '2' => "3A",
            >= '3' and <= '5' => "3B",
            >= '6' and <= '8' => "4A",
            >= '9' and <= '9' => "4B",
            _ => throw new UnreachableException()
        });
        Speak(Currency((fc ? 2 : 1) * (sn[3] switch
        {
            'a' or 'b' or 'c' => 2399,
            'd' or 'e' or 'f' => 9554,
            'g' or 'h' or 'i' => 5311,
            'j' or 'k' or 'l' => 1083,
            'm' or 'n' or 'p' => 512,
            'q' or 'r' or 's' => 10233,
            't' or 'u' or 'v' => 7600,
            'w' or 'x' or 'y' => 1422,
            'z' or '0' or '1' => 8890,
            '2' or '3' or '4' => 12144,
            '5' or '6' or '7' => 198,
            '8' or '9' => 3308,
            _ => throw new UnreachableException()
        })));

        ExitSubmenu();
        Solve();
    }

    private static string Currency(int v) => (v / 100) + " Euros and " + (v % 100) + " cents";

    private static string Depart(string color, char letter) => (letter switch
    {
        'a' or 'b' or 'c' => (string[])["Zwolle", "Swansea", "Ulm Hbf.", "Clermont-Ferrand", "Santander", "Antwerpen-Zuid"],
        'd' or 'e' or 'f' => ["Groningen", "Coventry", "Emden Hbf.", "Bordeaux St-Jean", "Ferrol", "Lokeren"],
        'g' or 'h' or 'i' => ["Amsterdam CS", "Peter­borough", "Cottbus", "Lille", "Plasencia", "Tielen"],
        'j' or 'k' or 'l' => ["Utrecht CS", "Cambridge", "Erfurt Hbf.", "Montargis", "Córdoba", "Hasselt"],
        'm' or 'n' or 'p' => ["Den Haag CS", "Stoke-on-Trent", "Kiel Hbf.", "Grenoble", "Almería", "Sint-Joris-Weert"],
        'q' or 'r' or 's' => ["Zutphen", "Watford Junction", "Potsdam Hbf.", "Cannes", "Gandía", "Waregem"],
        't' or 'u' or 'v' => ["Maastricht", "Exeter", "Ingolstadt Hbf.", "Redon", "Albacete", "Oostende"],
        'w' or 'x' or 'y' => ["Schiphol A’port", "Portsmouth H’bour", "Berlin Ost.", "Biarritz", "Aranjuez", "Enghien"],
        'z' or '0' or '1' => ["Delft", "Heathrow A’port", "Mainz Hbf.", "Limoges", "Cádiz", "Lierde"],
        '2' or '3' or '4' => ["Alkmaar", "Luton", "Frankfurt F’hafen", "Rouen-Rive-Droite", "Jaca", "Brussel-Zuid"],
        '5' or '6' or '7' => ["Lelystad Zuid", "Dover", "Regensburg Hbf.", "Le Havre", "Vitoria", "Halle"],
        '8' or '9' => ["Kampen", "Brighton", "Oberstdorf", "Dijon-Ville", "Murcia del Carmen", "Gent-Sint-Pieters"],
        _ => throw new UnreachableException()
    })[_colors.IndexOf(color)];

    private static string Destination(string color, char letter) => (letter switch
    {
        'a' or 'b' or 'c' => (string[])["Gouda", "Bristol Temple Meads", "Leipzig Hbf.", "C. De Gaulle A’port", "Girona", "Charleroi-Sud"],
        'd' or 'e' or 'f' => ["Leiden CS", "Pembroke Dock", "Augsburg Hbf.", "St-Dizier", "Soria", "Aarschot"],
        'g' or 'h' or 'i' => ["Leeuwarden", "London St. Pancras", "Bonn Hbf.", "Boulogne-Ville", "Ourense-Empalme", "Mechelen"],
        'j' or 'k' or 'l' => ["Middelburg", "Aylesbury", "Leer (Ostfriesl)", "Paris Gare du Nord", "Zafra", "Leuven"],
        'm' or 'n' or 'p' => ["Rotterdam CS", "Chester", "Bielefeld Hbf.", "Poitiers", "Málaga", "Spa"],
        'q' or 'r' or 's' => ["Deurne", "Bangor", "Chemnitz Hbf.", "Angers-Saint-Laud", "San Sebastián", "Idegem"],
        't' or 'u' or 'v' => ["Deventer", "Stour­bridge Town", "Karlsruhe Hbf.", "Nancy-Ville", "Reus", "Tongeren"],
        'w' or 'x' or 'y' => ["Assen", "Nottingham", "Freiburg Hbf.", "Lisieux", "Barcelona Sants", "Villers-La-Ville"],
        'z' or '0' or '1' => ["Eindhoven", "Manchester Victoria", "Lübeck Hbf.", "Marseille St-Charles", "Tarragona", "De Panne"],
        '2' or '3' or '4' => ["Nijmegen", "Sheffield", "Witten­berge", "Toul", "Guada­lajara", "Knokke"],
        '5' or '6' or '7' => ["Zandvoort aan Zee", "Wolver­hampton", "Dessau Hbf.", "Perpignan", "Madrid Atocha", "Zeebrugge-Strand"],
        '8' or '9' => ["Kerkrade Centrum", "Hull", "Jena Paradies", "Nîmes", "Linares-Baeza", "Kortrijk"],
        _ => throw new UnreachableException()
    })[_colors.IndexOf(color)];
}
