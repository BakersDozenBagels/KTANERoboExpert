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

    internal static event Func<Edgework> OnQueryEdgework = () => Edgework.Unspecified;
    internal static Edgework QueryEdgework() => OnQueryEdgework();

    internal static event Action<RoboExpertModule.EdgeworkType, Action> OnRequestEdgeworkFill = (_, _) => { };
    internal static void RequestEdgeworkFill(RoboExpertModule.EdgeworkType type, Action callback) => OnRequestEdgeworkFill(type, callback);
}