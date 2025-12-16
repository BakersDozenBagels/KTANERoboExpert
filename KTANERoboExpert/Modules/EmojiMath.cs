using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class EmojiMath : RoboExpertModule
{
    public override string Name => "Emoji Math";
    public override string Help => "happy colon equals sad plus colon neutral colon neutral";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices(_digits), 1, 2) + new Choices("plus", "minus") + new GrammarBuilder(new Choices(_digits), 1, 2));

    private static readonly string[] _digits = ["colon happy", "equals sad", "happy colon", "sad equals", "colon sad", "sad colon", "equals happy", "happy equals", "colon neutral", "neutral colon"];

    public override void ProcessCommand(string command)
    {
        var parts = command.Split(' ');

        int res = parts switch
        {
            [var a, var b, var c, var d, "plus", var e, var f, var g, var h] => _digits.IndexOf(a + " " + b) * 10 + _digits.IndexOf(c + " " + d) + _digits.IndexOf(e + " " + f) * 10 + _digits.IndexOf(g + " " + h),
            [var c, var d, "plus", var e, var f, var g, var h] => _digits.IndexOf(c + " " + d) + _digits.IndexOf(e + " " + f) * 10 + _digits.IndexOf(g + " " + h),
            [var a, var b, var c, var d, "plus", var g, var h] => _digits.IndexOf(a + " " + b) * 10 + _digits.IndexOf(c + " " + d) + _digits.IndexOf(g + " " + h),
            [var c, var d, "plus", var g, var h] => _digits.IndexOf(c + " " + d) + _digits.IndexOf(g + " " + h),
            [var a, var b, var c, var d, "minus", var e, var f, var g, var h] => _digits.IndexOf(a + " " + b) * 10 + _digits.IndexOf(c + " " + d) - _digits.IndexOf(e + " " + f) * 10 - _digits.IndexOf(g + " " + h),
            [var c, var d, "minus", var e, var f, var g, var h] => _digits.IndexOf(c + " " + d) - _digits.IndexOf(e + " " + f) * 10 - _digits.IndexOf(g + " " + h),
            [var a, var b, var c, var d, "minus", var g, var h] => _digits.IndexOf(a + " " + b) * 10 + _digits.IndexOf(c + " " + d) - _digits.IndexOf(g + " " + h),
            [var c, var d, "minus", var g, var h] => _digits.IndexOf(c + " " + d) - _digits.IndexOf(g + " " + h),
            _ => throw new UnreachableException()
        };

        Speak((res < 0 ? "negative " : "") + (res < 0 ? -res : res));
        ExitSubmenu();
        Solve();
    }
}
