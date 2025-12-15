using System.Speech.Recognition;

namespace KTANERoboExpert;

internal static class RoboExpertAPI
{
    internal static event Action<string, bool, bool> OnSendMessage = (_, _, _) => { };
    internal static void SendMessage(string s, bool ssml = false, bool async = false) => OnSendMessage(s, ssml, async);

    internal static event Action<RoboExpertModule, Grammar> OnEnterSubmenu = (_, _) => { };
    internal static void EnterSubmenu(RoboExpertModule m, Grammar s) => OnEnterSubmenu(m, s);

    internal static event Action OnExitSubmenu = () => { };
    internal static void ExitSubmenu() => OnExitSubmenu();

    internal static event Func<Edgework> OnQueryEdgework = () => Program.UnspecifiedEdgework;
    internal static Edgework QueryEdgework() => OnQueryEdgework();
}