using System.Speech.Recognition;

namespace KTANERoboExpert;

/// <summary>
/// A derived class defines the solver logic for a specific module.
/// </summary>
public abstract class RoboExpertModule
{
    /// <summary>
    /// Send a message to the user.
    /// </summary>
    /// <param name="message">The message to send.</param>
    protected static void Speak(string message) => RoboExpertAPI.SendMessage(message, async: true);
    /// <summary>
    /// Send a message to the user synchronously.
    /// </summary>
    /// <remarks>
    /// Prefer using <see cref="Speak"/> when possible.
    /// </remarks>
    /// <param name="message">The message to send.</param>
    protected static void SpeakSync(string message) => RoboExpertAPI.SendMessage(message);
    /// <summary>
    /// Send a message to the user using SSML.
    /// </summary>
    /// <param name="message">The message to send.</param>
    protected static void SpeakSSML(string message) => RoboExpertAPI.SendMessage(message, ssml: true, async: true);
    /// <summary>
    /// Send a message to the user synchronously using SSML.
    /// </summary>
    /// <remarks>
    /// Prefer using <see cref="SpeakSSML"/> when possible.
    /// </remarks>
    /// <param name="message">The message to send.</param>
    protected static void SpeakSSMLSync(string message) => RoboExpertAPI.SendMessage(message, ssml: true);

    /// <summary>
    /// Enters the provided submenu.
    /// </summary>
    /// <remarks>
    /// Submenus are useful for contextual information, such as the strip color in <see cref="Button"/>.
    /// </remarks>
    /// <param name="grammar">The <see cref="Grammar"/> to be used for the submenu.</param>
    protected void EnterSubmenu(Grammar grammar) => RoboExpertAPI.EnterSubmenu(this, grammar);
    /// <summary>
    /// Exits the current submenu.
    /// </summary>
    /// <remarks>
    /// This does NOT call <see cref="Cancel"/>.
    /// </remarks>
    protected static void ExitSubmenu() => RoboExpertAPI.ExitSubmenu();
    /// <summary>
    /// Gets the bomb's current edgework.
    /// </summary>
    protected static Edgework Edgework => RoboExpertAPI.QueryEdgework();
    /// <summary>
    /// Requests a piece of edgework from the user.
    /// </summary>
    /// <param name="type">The piece of edgework to request.</param>
    /// <param name="callback">The callback to be invoked once the edgework has been supplied.</param>
    /// <param name="onCancel">The callback to be invoked if the edgework is not supplied.</param>
    protected static void RequestEdgeworkFill(EdgeworkType type, Action? callback = null, Action? onCancel = null) => RoboExpertAPI.RequestEdgeworkFill(type, callback ?? (() => { }), onCancel ?? (() => { }));
    /// <summary>
    /// The NATO phonetic alphabet.
    /// </summary>
    protected internal static readonly IReadOnlyCollection<string> NATO = ["alfa", "bravo", "charlie", "delta", "echo", "foxtrot", "golf", "hotel", "india", "juliet", "kilo", "lima", "mike", "november", "oscar", "papa", "quebec", "romeo", "sierra", "tango", "uniform", "victor", "whiskey", "xray", "yankee", "zulu"];

    /// <summary>
    /// The name of the module this class solves.
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// A help message describing to the user how to use this solver.
    /// </summary>
    public abstract string Help { get; }

    /// <summary>
    /// The base <see cref="Grammar"/> used by default when interacting with this module.
    /// </summary>
    /// <remarks>
    /// This defines all the valid commands for a given solver.<br/>
    /// If a command is only sometimes valid, it should probably be specified in a submenu.
    /// </remarks>
    /// <seealso cref="EnterSubmenu(Grammar)"/>
    public abstract Grammar Grammar { get; }

    /// <summary>
    /// Called when the solver is selected.
    /// </summary>
    public virtual void Select() => Speak("Go on " + Name);
    /// <summary>
    /// Called when the solver should handle a command.
    /// </summary>
    /// <param name="command">
    /// The command to handle.<br/>This will conform to either the
    /// base <see cref="System.Speech.Recognition.Grammar"/> (<see cref="Grammar"/>)
    /// outside of a submenu or whichever <see cref="System.Speech.Recognition.Grammar"/>
    /// was specified for the current innermost submenu.
    /// </param>
    public abstract void ProcessCommand(string command);
    /// <summary>
    /// Called when the user says "cancel" to exit out of one of this solver's
    /// submenus or out of this solver entirely.<br/>
    /// This is NOT called as a result of <see cref="ExitSubmenu"/>.
    /// </summary>
    public virtual void Cancel() { }
    /// <summary>
    /// Called when the user says "reset" to reset the program.
    /// This should clear any residual state.
    /// </summary>
    public virtual void Reset() { }

    /// <summary>A type of edgework.</summary>
    protected internal enum EdgeworkType
    {
        /// <summary>The bomb's serial number.</summary>
        SerialNumber,
        /// <summary>The bomb's battery count and battery holder count.</summary>
        Batteries,
        /// <summary>The bomb's indicators.</summary>
        Indicators,
        /// <summary>The bomb's ports and ports plates.</summary>
        Ports
    }
}
