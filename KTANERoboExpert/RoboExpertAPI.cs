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

    internal static event Action<string> OnSolve = s => { };
    internal static void Solve(string module) => OnSolve(module);

    internal static event Action<Action<string?>> OnRegisterSolveHandler = s => { };
    internal static void RegisterSolveHandler(Action<string?> handler) => OnRegisterSolveHandler(handler);
    internal static event Action<Action<string?>> OnUnregisterSolveHandler = s => { };
    internal static void UnregisterSolveHandler(Action<string?> handler) => OnUnregisterSolveHandler(handler);

    internal static event Action<Action<Action>> OnInterrupt = c => { };
    internal static void Interrupt(Action<Action> callback) => OnInterrupt(callback);

    internal static event Action<Action> OnLoad = a => { };
    internal static void Load(Action a) => OnLoad(a);
}