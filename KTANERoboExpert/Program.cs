using KTANERoboExpert;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

internal static partial class Program
{
    private static readonly SpeechSynthesizer _speaker;
    private static readonly SpeechRecognitionEngine _microphone;
    private static Grammar _defaultGrammar, _globalGrammar, _edgeworkGrammar;
    private static readonly List<Grammar> _edgeworkGrammars = [];
    private static bool _listening, _resetConfirm, _subtitlesOn;
    private static readonly Stack<Context> _contexts = [];
    private static RoboExpertModule[] _modules;
    private static Edgework _edgework = Edgework.Unspecified;
    private static Action _edgeworkCallback = () => { };
    private static Action? _edgeworkCancel;
    private static RoboExpertModule.EdgeworkType? _edgeworkQuery;

#if DEBUG
    private static bool _micOn;
#endif

    private record struct Context(RoboExpertModule? Module, Grammar Grammar);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    static Program()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _speaker = new SpeechSynthesizer();
        _microphone = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-us"));
    }

    private static void Speak(string m)
    {
        _speaker.SpeakAsync(m);
        if (_subtitlesOn)
            Console.WriteLine(m);
    }

    private static void Main(string[] args)
    {
        _speaker.SetOutputToDefaultAudioDevice();
        _speaker.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);

        _microphone.SetInputToDefaultAudioDevice();
        _microphone.SpeechRecognized += Recognized;
        _microphone.RecognizerUpdateReached += FlushLoad;

        _listening = false;
        RoboExpertAPI.OnSendMessage += (m, s, a) =>
        {
            (a ? new Action<Prompt>(_speaker.Speak) : new Action<Prompt>(_speaker.SpeakAsync))
            .Invoke(new Prompt(s ? $"<speak version=\"1.0\" xml:lang=\"en-US\">{m}</speak>" : m, s ? SynthesisTextFormat.Ssml : SynthesisTextFormat.Text));
            if (_subtitlesOn)
                Console.WriteLine(m);
        };
        RoboExpertAPI.OnExitSubmenu += () => ExitSubmenu(false);
        RoboExpertAPI.OnQueryEdgework += () => _edgework;
        RoboExpertAPI.OnEnterSubmenu += (m, g) =>
        {
            Load(() =>
            {
                if (_contexts.Count == 0)
                    _defaultGrammar.Enabled = false;
                else
                    _contexts.Peek().Grammar.Enabled = false;
                if (!_microphone.Grammars.Contains(g))
                    _microphone.LoadGrammar(g);
                else
                    g.Enabled = true;
                _contexts.Push(new Context(m, g));
            });
        };

        _modules = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => typeof(RoboExpertModule).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<RoboExpertModule>()
            .ToArray();

        var moduleName = new Choices();
        foreach (var m in _modules)
            moduleName.Add(m.Name);

        var selectModule = new GrammarBuilder("module");
        selectModule.Append(moduleName);


        var all = new Choices();
        all.Add(selectModule);
        all.Add("edgework");
        all.Add("reset");
        _defaultGrammar = new Grammar(all.ToGrammarBuilder());
        _globalGrammar = new Grammar(new GrammarBuilder(new Choices("cancel", "strike")));

        var digit = new Choices();
        var letter = new Choices();
        var character = new Choices();
        foreach (var c in Enumerable.Range(0, 10).Select(x => x.ToString()))
        {
            digit.Add(c);
            character.Add(c);
        }
        foreach (var c in RoboExpertModule.NATO)
        {
            letter.Add(c);
            character.Add(c);
        }
        var number = new Choices(Enumerable.Range(0, 100).Select(x => x.ToString()).ToArray());

        var serialb = new GrammarBuilder();
        var serialb2 = new GrammarBuilder("serial");
        serialb.Append(character);
        serialb.Append(character);
        serialb.Append(digit);
        serialb.Append(letter);
        serialb.Append(letter);
        serialb.Append(digit);
        serialb2.Append(character);
        serialb2.Append(character);
        serialb2.Append(digit);
        serialb2.Append(letter);
        serialb2.Append(letter);
        serialb2.Append(digit);
        var serial = new Grammar(serialb);
        _edgeworkGrammars.Add(serial);

        var batteriesb = new GrammarBuilder();
        batteriesb.Append(number);
        batteriesb.Append("batteries");
        batteriesb.Append(number);
        batteriesb.Append("holders");
        var batteries = new Grammar(batteriesb);
        _edgeworkGrammars.Add(batteries);

        var strike = new GrammarBuilder();
        strike.Append("strike");
        strike.Append(number);

        var indicator = new GrammarBuilder();
        indicator.Append(new Choices("lit", "unlit"));
        indicator.Append(new Choices("b o b", "c a r", "c l r", "f r k", "f r q", "i n d", "mike s a", "november s a", "s i g", "s n d", "t r n", "n l l"));

        var indicatorsb = new GrammarBuilder();
        indicatorsb.Append(indicator, 1, 10);
        indicatorsb.Append("done");
        var indicatorsc = new Choices(indicatorsb);
        indicatorsc.Add("no indicators");
        var indicators = new Grammar(indicatorsc);
        _edgeworkGrammars.Add(indicators);

        var edgeworkPieces = new Choices();
        edgeworkPieces.Add(serialb2);
        edgeworkPieces.Add(batteriesb);
        edgeworkPieces.Add(strike);
        edgeworkPieces.Add(indicatorsc);

        RoboExpertAPI.OnRequestEdgeworkFill += (type, callback, onCancel) =>
        {
            if (type switch
            {
                RoboExpertModule.EdgeworkType.SerialNumber => _edgework.SerialNumber as object,
                RoboExpertModule.EdgeworkType.Batteries => _edgework.Batteries,
                RoboExpertModule.EdgeworkType.Indicators => _edgework.Indicators,
                RoboExpertModule.EdgeworkType.Ports => _edgework.Ports,
                _ => throw new ArgumentException("Bad edgework type", nameof(type)),
            } is not null)
            {
                callback();
                return;
            }

            Load(() =>
            {
                if (_contexts.Count == 0)
                    _defaultGrammar.Enabled = false;
                else
                    _contexts.Peek().Grammar.Enabled = false;
                Grammar g = type switch
                {
                    RoboExpertModule.EdgeworkType.SerialNumber => serial,
                    RoboExpertModule.EdgeworkType.Batteries => batteries,
                    RoboExpertModule.EdgeworkType.Indicators => indicators,
                    RoboExpertModule.EdgeworkType.Ports => throw new NotImplementedException(),
                    _ => throw new ArgumentException("Bad edgework type", nameof(type)),
                };
                _edgeworkQuery = type;
                Speak(type switch
                {
                    RoboExpertModule.EdgeworkType.SerialNumber => "What's the serial number?",
                    RoboExpertModule.EdgeworkType.Batteries => "What are the batteries?",
                    RoboExpertModule.EdgeworkType.Indicators => "What are the indicators?",
                    RoboExpertModule.EdgeworkType.Ports => "What are the ports?",
                    _ => throw new ArgumentException("Bad edgework type", nameof(type)),
                });
                if (!_microphone.Grammars.Contains(g))
                    _microphone.LoadGrammar(g);
                else
                    g.Enabled = true;
                _contexts.Push(new Context(null, g));

                _edgeworkCallback = callback;
                _edgeworkCancel = onCancel;
            });
        };

        _edgeworkGrammar = new Grammar(edgeworkPieces.ToGrammarBuilder());
        _edgeworkGrammars.Add(_edgeworkGrammar);

        _microphone.LoadGrammar(_defaultGrammar);
        _microphone.LoadGrammar(_globalGrammar);

        _microphone.RecognizeAsync(RecognizeMode.Multiple);
#if DEBUG
        _micOn = true;
#endif
        foreach (var arg in args)
            if (!ProcessCommand(arg))
                return;

#if DEBUG
        ProcessCommand("subtitles");
        ProcessCommand("start");
#endif

        while (ProcessCommand(Console.ReadLine() ?? "")) { }
    }

    private static bool ProcessCommand(string cmd)
    {
        cmd = cmd.ToLowerInvariant().Trim();
#if DEBUG
        if (cmd.StartsWith("c ") || cmd.StartsWith("command "))
        {
            if (cmd.StartsWith("c "))
                cmd = cmd[2..];
            else cmd = cmd[8..];
            _microphone.RecognizeAsyncStop();
            Thread.Sleep(100);
            _micOn = false;
            _microphone.EmulateRecognize(cmd);
            return true;
        }
        if (cmd.StartsWith("s ") || cmd.StartsWith("say "))
        {
            if (cmd.StartsWith("s "))
                cmd = cmd[2..];
            else cmd = cmd[4..];
            Speak(cmd);
            return true;
        }
#endif
        switch (cmd)
        {
            case "--start":
            case "start":
                if (!_listening)
                    Speak("Hello");
                _listening = true;
#if DEBUG
                if (!_micOn)
                    _microphone.RecognizeAsync(RecognizeMode.Multiple);
#endif
                break;
            case "--stop":
            case "stop":
                if (_listening)
                    Speak("Goodbye");
                _listening = false;
                break;
            case "quit":
            case "exit":
            case "q":
            case "x":
                Speak("Shutting down");
                return false;
            case "--help":
            case "help":
            case "-h":
            case "h":
            case "-?":
            case "?":
                Console.WriteLine("""
                start       Start listening
                stop        Stop listening
                Help        Shows this help message
                Quit        Quits the program
                subTitles   Enables subtitles
                Nosubtitles Disables subtitles
                """
#if DEBUG
                + """

                Pause       Break the debugger
                Command     Send a command as text
                Say         Say a message
                """
#endif
                );
                break;
#if DEBUG
            case "pause":
            case "break":
            case "p":
            case "b":
                Debugger.Break();
                break;
#endif
            case "--subtitles":
            case "subtitles":
            case "-t":
            case "t":
                _subtitlesOn = true;
                break;
            case "--no-subtitles":
            case "--nosubtitles":
            case "nosubtitles":
            case "-n":
            case "n":
                _subtitlesOn = false;
                break;
            default:
                Console.WriteLine("Unknown command. Use 'help' for a list of commands.");
                break;
        }
        return true;
    }

    private static void Recognized(object? sender, SpeechRecognizedEventArgs e)
    {
        if (!_listening) return;

        if (e.Result.Grammar != _defaultGrammar || !e.Result.Text.StartsWith("reset"))
            _resetConfirm = false;

        if (e.Result.Grammar == _globalGrammar)
        {
            if (e.Result.Text == "cancel")
            {
                if (_contexts.Count == 0)
                {
                    Speak("nothing to cancel");
                }
                else
                {
                    _contexts.Peek().Module?.Cancel();
                    ExitSubmenu();
                    Load(() => Speak("cancelled"));
                    _edgeworkCancel?.Invoke();
                    _edgeworkCancel = null;
                }
            }
            else if (e.Result.Text == "strike")
            {
                _edgework = _edgework with { Strikes = _edgework.Strikes + 1 };
                Speak("strike " + _edgework.Strikes);
            }
        }
        else if (_edgeworkGrammars.Contains(e.Result.Grammar))
        {
            FillEdgework(e.Result.Text, e.Result.Grammar != _edgeworkGrammar ? _edgeworkCallback : null);
            _edgeworkCancel = null;
        }
        else if (e.Result.Grammar == _defaultGrammar)
        {
            var command = e.Result.Text;
            if (command.StartsWith("module"))
            {
                command = command[7..];
                var mod = _modules.FirstOrDefault(m => m.Name == command);
                if (mod == null) return;
                Load(() =>
                {
                    if (_microphone.Grammars.Contains(mod.Grammar))
                        mod.Grammar.Enabled = true;
                    else
                        _microphone.LoadGrammar(mod.Grammar);
                    _defaultGrammar.Enabled = false;
                    _contexts.Push(new Context(mod, mod.Grammar));
                    mod.Select();
                });
            }
            else if (command.StartsWith("edgework"))
            {
                Load(() =>
                {
                    _defaultGrammar.Enabled = false;
                    if (_microphone.Grammars.Contains(_edgeworkGrammar))
                        _edgeworkGrammar.Enabled = true;
                    else
                        _microphone.LoadGrammar(_edgeworkGrammar);
                    _contexts.Push(new Context(null, _edgeworkGrammar));
                    Speak("Go on edgework");
                });
            }
            else if (command.StartsWith("reset"))
            {
                if (!_resetConfirm)
                {
                    Speak("Are you sure?");
                    _resetConfirm = true;
                }
                else
                {
                    _edgework = Edgework.Unspecified;
                    foreach (var m in _modules)
                        m.Reset();
                    Speak("Bomb has been reset");
                    ExitSubmenu(all: true);
                    _resetConfirm = false;
                }
            }
        }
        else if (_contexts.Count > 0)
        {
            try
            {
                _contexts.Peek().Module!.ProcessCommand(e.Result.Text);
            }
            catch (Exception ex)
            {
                Speak("There has been an error.");
                Console.WriteLine(ex);
                ExitSubmenu(all: true);
            }
        }
        else
        {
            Speak("Error");
        }
    }
    private static void ExitSubmenu(bool all = false)
    {
        Load(() =>
        {
            if (_contexts.Count == 0)
                return;
            _contexts.Pop().Grammar.Enabled = false;
            if (all)
                _contexts.Clear();
            if (_contexts.Count == 0)
                _defaultGrammar.Enabled = true;
            else
                _contexts.Peek().Grammar.Enabled = true;
            _edgeworkQuery = null;
        });
    }

    private static void FillEdgework(string command, Action? callback = null)
    {
        if (_edgeworkQuery == RoboExpertModule.EdgeworkType.SerialNumber || (_edgeworkQuery == null && command.StartsWith("serial")))
        {
            var lookup = RoboExpertModule.NATO.Concat(Enumerable.Range(0, 10).Select(i => i.ToString())).ToArray();
            if (command.Split(' ').Length != (_edgeworkQuery == RoboExpertModule.EdgeworkType.SerialNumber ? 6 : 7))
                return;
            var alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-";
            _edgework = _edgework with
            {
                SerialNumber = command
                .Split(' ')
                .Skip(_edgeworkQuery == RoboExpertModule.EdgeworkType.SerialNumber ? 0 : 1)
                .Select(s => alpha[Array.IndexOf(lookup, s) is var x && x is -1 ? 36 : x].ToString())
                .Join()
            };

            Speak(_edgework.SerialNumber.Select(c => c.ToString()).Join(" "));
        }
        if (_edgeworkQuery == RoboExpertModule.EdgeworkType.Batteries || (_edgeworkQuery == null && BatteryRegex().IsMatch(command)))
        {
            var match = BatteryRegex().Match(command);
            _edgework = _edgework with
            {
                Batteries = int.Parse(match.Groups[1].Value),
                BatteryHolders = int.Parse(match.Groups[2].Value)
            };

            Speak(_edgework.Batteries + " in " + _edgework.BatteryHolders);
        }
        else if (_edgeworkQuery == RoboExpertModule.EdgeworkType.Indicators || (_edgeworkQuery == null && (IndicatorsRegex().IsMatch(command) || NoIndicatorsRegex().IsMatch(command))))
        {
            if (NoIndicatorsRegex().IsMatch(command))
            {
                _edgework = _edgework with { Indicators = [] };
            }
            else
            {
                _edgework = _edgework with
                {
                    Indicators = IndicatorsRegex()
                    .Matches(command)
                    .Select(m => new Edgework.Indicator(m.Groups[2].Value.Replace(" ", "").ToUpperInvariant() is var ind && ind == "MIKESA" ? "MSA" : ind == "NOVEMBERSA" ? "NSA" : ind, m.Groups[1].Value == "lit"))
                    .ToList()
                };
            }

            Speak(_edgework.Indicators.Count + " indicator" + (_edgework.Indicators.Count == 1 ? "" : "s"));
        }
        else if (command.StartsWith("strike"))
        {
            _edgework = _edgework with { Strikes = int.Parse(command[7..]) };
            Speak(command[7..] + "strikes");
        }

        if (callback != null)
        {
            ExitSubmenu();
            callback.Invoke();
            _edgeworkCancel = null;
        }
    }

    private static readonly Queue<Action> _toLoad = new();

    private static void Load(Action callback)
    {
        _toLoad.Enqueue(callback);
        _microphone.RequestRecognizerUpdate();
    }

    private static void FlushLoad(object? sender, RecognizerUpdateReachedEventArgs e)
    {
        while (_toLoad.Count > 0)
            _toLoad.Dequeue()();
    }

    [GeneratedRegex("(\\d+) batteries (\\d+) holders", RegexOptions.Compiled)]
    private static partial Regex BatteryRegex();

    [GeneratedRegex("(lit|unlit) (b o b|c a r|c l r|f r k|f r q|i n d|mike s a|november s a|s i g|s n d|t r n|n l l)", RegexOptions.Compiled)]
    private static partial Regex IndicatorsRegex();
    [GeneratedRegex("no indicators", RegexOptions.Compiled)]
    private static partial Regex NoIndicatorsRegex();
}