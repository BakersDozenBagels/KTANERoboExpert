using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class Listening : RoboExpertModule
{
    public override string Name => "Listening";
    public override string Help => "Taxi Dispatch";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices([.. _table.Keys])));

    public override void ProcessCommand(string command)
    {
        Speak(_table[command].Select(c => c switch { '$' => "dollar", '#' => "pound", '&' => "ampersand", '*' => "asterisk", _ => throw new UnreachableException() }).Conjoin());
        ExitSubmenu();
        Solve();
    }

    private static readonly Dictionary<string, string> _table = new()
    {
        ["Taxi Dispatch"] = "&&&**",
        ["Cow"] = "&$#$&",
        ["Extractor Fan"] = "$#$*&",
        ["Train Station"] = "#$$**",
        ["Arcade"] = "$#$#*",
        ["Casino"] = "**$*#",
        ["Supermarket"] = "#$$&*",
        ["Soccer Match"] = "##*$*",
        ["Tawny Owl"] = "$#*$&",
        ["Sewing Machine"] = "#&&*#",
        ["Thrush Nightingale"] = "**#**",
        ["Car Engine"] = "&#**&",
        ["Reloading Glock 19"] = "$&**#",
        ["Reloading Glock"] = "$&**#",
        ["Reloading Gun"] = "$&**#",
        ["Oboe"] = "&#$$#",
        ["Saxophone"] = "$&&**",
        ["Tuba"] = "#&$##",
        ["Marimba"] = "&*$*$",
        ["Phone Ringing"] = "&$$&*",
        ["Telephone Ringing"] = "&$$&*",
        ["Ringing Phone"] = "&$$&*",
        ["Ringing Telephone"] = "&$$&*",
        ["Phone"] = "&$$&*",
        ["Telephone"] = "&$$&*",
        ["Tibetan Nuns"] = "#&&&&",
        ["Throat Singing"] = "**$$$",
        ["Beach"] = "*&*&&",
        ["Dial-up Internet"] = "*#&*&",
        ["Police Radio Scanner"] = "**###",
        ["Police Scanner"] = "**###",
        ["Radio Scanner"] = "**###",
        ["Censorship Bleep"] = "&&$&*",
        ["Censor Bleep"] = "&&$&*",
        ["Medieval Weapons"] = "&$**&",
        ["Door Closing"] = "#$#&$",
        ["Closing Door"] = "#$#&$",
        ["Chainsaw"] = "&#&&#",
        ["Compressed Air"] = "$$*$*",
        ["Servo Motor"] = "$&#$$",
        ["Waterfall"] = "&**$$",
        ["Tearing Fabric"] = "$&&*&",
        ["Fabric Tearing"] = "$&&*&",
        ["Zipper"] = "&$&##",
        ["Vacuum Cleaner"] = "#&$*&",
        ["Vacuum"] = "#&$*&",
        ["Ballpoint Pen Writing"] = "$*$**",
        ["Ballpoint Pen"] = "$*$**",
        ["Pen"] = "$*$**",
        ["Rattling Iron Chain"] = "*#$&&",
        ["Rattling Chain"] = "*#$&&",
        ["Iron Chain"] = "*#$&&",
        ["Chain"] = "*#$&&",
        ["Book Page Turning"] = "###&$",
        ["Page Turning"] = "###&$",
        ["Turning Page"] = "###&$",
        ["Book Page"] = "###&$",
        ["Table Tennis"] = "*$$&$",
        ["Ping Pong"] = "*$$&$",
        ["Squeaky Toy"] = "$*&##",
        ["Dog Toy"] = "$*&##",
        ["Helicopter"] = "#&$&&",
        ["Firework Exploding"] = "$&$$*",
        ["Firework"] = "$&$$*",
        ["Explosion"] = "$&$$*",
        ["Glass Shattering"] = "*$*$*",
    };
}
