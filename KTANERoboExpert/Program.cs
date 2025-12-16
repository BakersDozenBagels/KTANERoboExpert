#if DEBUG
using System.Diagnostics;
#endif
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using KTANERoboExpert;
using KTANERoboExpert.Uncertain;

internal static partial class Program
{
    private static readonly SpeechSynthesizer _speaker;
    private static readonly SpeechRecognitionEngine _microphone;
    private static Grammar _defaultGrammar, _globalGrammar, _edgeworkGrammar;
    private static readonly List<Grammar> _edgeworkGrammars = [];
    private static bool _listening, _resetConfirm, _subtitlesOn;
    private static readonly Stack<Context> _contexts = [];
    private static RoboExpertModule[] _modules;
    private static Edgework _edgework = UnspecifiedEdgework;
    private static Action _edgeworkCallback = () => { };
    private static Action? _edgeworkCancel;
    private static RoboExpertModule.EdgeworkType? _edgeworkQuery;
    private static Action<RoboExpertModule.EdgeworkType, Action, Action?> _onRequestEdgeworkFill;

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

        RoboExpertAPI.OnSolve += HandleSolve;
        RoboExpertAPI.OnRegisterSolveHandler += h => _onSolveHandlers.Add(h);
        RoboExpertAPI.OnUnregisterSolveHandler += h => _onSolveHandlers.Remove(h);

        Queue<Action<Action>> interrupts = [];
        Action yield(Action<Action> c) => () =>
            {
                if (interrupts.Count is 0 || interrupts.Peek() != c)
                    return;
                interrupts.Dequeue();
                if (interrupts.Count is not 0)
                    interrupts.Peek()(yield(interrupts.Peek()));
            };
        RoboExpertAPI.OnInterrupt += c =>
        {
            interrupts.Enqueue(c);
            if (interrupts.Count is 1)
                c(yield(c));
        };

        RoboExpertAPI.OnLoad += Load;

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
        all.Add("solve");
        all.Add("pause");
        all.Add("unpause");
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
        var number = new Choices(RoboExpertModule.Numbers.ToArray());

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

        var psPlate = new Choices("parallel", "serial", "parallel serial", "serial parallel");
        var fullPlate = new GrammarBuilder(new Choices("DVI", "PS", "RJ", "RCA"), 1, 4);
        var anyPlate = new Choices(psPlate, fullPlate, "empty");
        var ports = new Choices("none", new GrammarBuilder("plate" + anyPlate.ToGrammarBuilder(), 1, 10) + "done");
        var portsGrammar = new Grammar(ports);
        var portsMenu = "ports" + ports.ToGrammarBuilder();
        _edgeworkGrammars.Add(portsGrammar);

        var modulesb = new GrammarBuilder();
        modulesb.Append(number);
        modulesb.Append("modules");
        var modules = new Grammar(modulesb);
        _edgeworkGrammars.Add(modules);

        var solvesb = new GrammarBuilder();
        solvesb.Append(number);
        solvesb.Append("solves");
        var solves = new Grammar(solvesb);
        _edgeworkGrammars.Add(solves);

        var edgeworkPieces = new Choices();
        edgeworkPieces.Add(serialb2);
        edgeworkPieces.Add(batteriesb);
        edgeworkPieces.Add(strike);
        edgeworkPieces.Add(indicatorsc);
        edgeworkPieces.Add(portsMenu);
        edgeworkPieces.Add(modulesb);
        edgeworkPieces.Add(solvesb);

        _edgeworkGrammar = new Grammar(edgeworkPieces.ToGrammarBuilder());
        _edgeworkGrammars.Add(_edgeworkGrammar);

        _onRequestEdgeworkFill = (type, callback, onCancel) =>
        {
            if ((type switch
            {
                RoboExpertModule.EdgeworkType.SerialNumber => _edgework.SerialNumber as IUncertain,
                RoboExpertModule.EdgeworkType.Batteries => _edgework.Batteries,
                RoboExpertModule.EdgeworkType.Indicators => _edgework.Indicators,
                RoboExpertModule.EdgeworkType.Ports => _edgework.Ports,
                RoboExpertModule.EdgeworkType.Solves => _edgework.Solves,
                RoboExpertModule.EdgeworkType.ModuleCount => _edgework.ModuleCount,
                _ => throw new ArgumentException("Bad edgework type", nameof(type)),
            }).IsCertain)
            {
                if (callback is not null)
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
                    RoboExpertModule.EdgeworkType.Ports => portsGrammar,
                    RoboExpertModule.EdgeworkType.Solves => modules,
                    RoboExpertModule.EdgeworkType.ModuleCount => solves,
                    _ => throw new ArgumentException("Bad edgework type", nameof(type)),
                };
                _edgeworkQuery = type;
                Speak(type switch
                {
                    RoboExpertModule.EdgeworkType.SerialNumber => "What's the serial number?",
                    RoboExpertModule.EdgeworkType.Batteries => "What are the batteries?",
                    RoboExpertModule.EdgeworkType.Indicators => "What are the indicators?",
                    RoboExpertModule.EdgeworkType.Ports => "What are the ports?",
                    RoboExpertModule.EdgeworkType.Solves => "How many solves?",
                    RoboExpertModule.EdgeworkType.ModuleCount => "How many modules?",
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

        while (Console.ReadLine() is { } command && ProcessCommand(command)) { }
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

    private readonly static List<Action<string?>> _onSolveHandlers = [];
    private static void HandleSolve(string? module)
    {
        _edgework = _edgework with { Solves = _edgework.Solves + 1 };
        if (_onSolveHandlers.Count is not 0)
            Speak("Solve " + (_edgework.Solves.IsCertain ? _edgework.Solves.Value : _edgework.Solves.Min));
        foreach (var h in _onSolveHandlers)
            h(module);
    }

    private static void Recognized(object? sender, SpeechRecognizedEventArgs e)
    {
        if (e.Result.Grammar == _defaultGrammar && e.Result.Text == "unpause")
        {
            if (_subtitlesOn)
                Console.WriteLine("# " + e.Result.Text);
            if (!_listening)
                Speak("Hello");
            _listening = true;
            return;
        }

        if (!_listening) return;

        if (_subtitlesOn)
            Console.WriteLine("# " + e.Result.Text);

        if (e.Result.Grammar != _defaultGrammar || !e.Result.Text.StartsWith("reset"))
            _resetConfirm = false;

        if (e.Result.Grammar == _globalGrammar)
        {
            if (e.Result.Text == "cancel")
            {
                if (_contexts.Count == 0)
                    Speak("nothing to cancel");
                else
                {
                    var module = _contexts.Peek().Module;
                    ExitSubmenu();
                    Load(() => Speak("cancelled"));
                    module?.Cancel();
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
                    _edgework = UnspecifiedEdgework;
                    foreach (var m in _modules)
                        m.Reset();
                    Speak("Bomb has been reset");
                    ExitSubmenu(all: true);
                    _resetConfirm = false;
                }
            }
            else if (command.StartsWith("solve"))
                HandleSolve(null);
            else if (e.Result.Text == "pause")
            {
                if (_listening)
                    Speak("Goodbye");
                _listening = false;
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
                .Conjoin(string.Empty)
            };

            Speak(_edgework.SerialNumber.Value!.Select(c => c.ToString()).Conjoin());
        }
        else if (_edgeworkQuery == RoboExpertModule.EdgeworkType.Batteries || (_edgeworkQuery == null && BatteryRegex().IsMatch(command)))
        {
            var match = BatteryRegex().Match(command);
            _edgework = _edgework with
            {
                Batteries = int.Parse(match.Groups[1].Value),
                _batteryHolders = int.Parse(match.Groups[2].Value)
            };

            Speak(_edgework.Batteries.Value + " in " + _edgework.BatteryHolders.Value);
        }
        else if (_edgeworkQuery == RoboExpertModule.EdgeworkType.Indicators || (_edgeworkQuery == null && (IndicatorsRegex().IsMatch(command) || NoIndicatorsRegex().IsMatch(command))))
        {
            if (NoIndicatorsRegex().IsMatch(command))
                _edgework = _edgework with { Indicators = new([]) };
            else
            {
                _edgework = _edgework with
                {
                    Indicators = new([..IndicatorsRegex()
                    .Matches(command)
                    .Select(m => new Edgework.Indicator(m.Groups[2].Value.Replace(" ", "").ToUpperInvariant() is var ind && ind == "MIKESA" ? "MSA" : ind == "NOVEMBERSA" ? "NSA" : ind, m.Groups[1].Value == "lit"))])
                };
            }

            Speak(_edgework.Indicators.Count.Value + " indicator" + (_edgework.Indicators.Count.Value == 1 ? "" : "s"));
        }
        else if (_edgeworkQuery == RoboExpertModule.EdgeworkType.Ports || (_edgeworkQuery == null && command.StartsWith("ports ")))
        {
            if (command.StartsWith("ports "))
                command = command[6..];
            if (command == "none")
            {
                _edgework = _edgework with { _ports = new([]) };
                Speak("0 port plates");
            }
            else
            {
                static Maybe<Edgework.PortPlate> Parse(string parts)
                {
                    var ports = parts.Split(' ');
                    if (ports.Length != ports.Distinct().Count())
                        return new();
                    return new(new(
                        ports.Contains("DVI"), ports.Contains("parallel"),
                        ports.Contains("PS"), ports.Contains("RJ"),
                        ports.Contains("serial"), ports.Contains("RCA")
                    ));
                }
                var plates = command[..^5]
                    .Split("plate", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(Parse)
                    .ToArray();
                if (plates.Any(p => !p.Exists))
                    return;
                _edgework = _edgework with { _ports = new([.. plates.Select(x => x.Item)]) };
                Speak(plates.Length + " port plate" + (plates.Length > 1 ? "s" : ""));
            }
        }
        else if (_edgeworkQuery == RoboExpertModule.EdgeworkType.ModuleCount || (_edgeworkQuery == null && command.EndsWith(" modules")))
        {
            _edgework = _edgework with { ModuleCount = int.Parse(command[..^8]) };
            Speak(command[..^8] + " modules");
        }
        else if (_edgeworkQuery == RoboExpertModule.EdgeworkType.Solves || (_edgeworkQuery == null && command.EndsWith(" solves")))
        {
            _edgework = _edgework with { Solves = int.Parse(command[..^7]) };
            Speak(command[..^7] + " solves");
        }
        else if (_edgeworkQuery == null && command.StartsWith("strike"))
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

    internal static Edgework UnspecifiedEdgework
    {
        get => new(
            new((a, b) => _onRequestEdgeworkFill(RoboExpertModule.EdgeworkType.SerialNumber, a, b)),
            new((a, b) => _onRequestEdgeworkFill(RoboExpertModule.EdgeworkType.Batteries, a, b)),
            new((a, b) => _onRequestEdgeworkFill(RoboExpertModule.EdgeworkType.Batteries, a, b)),
            new((a, b) => _onRequestEdgeworkFill(RoboExpertModule.EdgeworkType.Indicators, a, b)),
            new((a, b) => _onRequestEdgeworkFill(RoboExpertModule.EdgeworkType.Ports, a, b)),
            0,
            UncertainInt.InRange(0, 0, (a, b) => _onRequestEdgeworkFill(RoboExpertModule.EdgeworkType.Solves, a, b)),
            UncertainInt.AtLeast(0, (a, b) => _onRequestEdgeworkFill(RoboExpertModule.EdgeworkType.ModuleCount, a, b)),
            5);
    }

    [GeneratedRegex("(\\d+) batteries (\\d+) holders", RegexOptions.Compiled)]
    private static partial Regex BatteryRegex();

    [GeneratedRegex("(lit|unlit) (b o b|c a r|c l r|f r k|f r q|i n d|mike s a|november s a|s i g|s n d|t r n|n l l)", RegexOptions.Compiled)]
    private static partial Regex IndicatorsRegex();
    [GeneratedRegex("no indicators", RegexOptions.Compiled)]
    private static partial Regex NoIndicatorsRegex();
}
