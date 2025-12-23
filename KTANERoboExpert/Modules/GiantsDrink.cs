using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class GiantsDrink : RoboExpertModule
{
    public override string Name => "Giant's Drink";
    public override string Help => "yes | no";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices("yes", "no"));

    private Maybe<bool>[] _state = new Maybe<bool>[5];
    private bool _registered;

    public override void ProcessCommand(string command)
    {
        for (int i = 0; i < _state.Length; i++)
        {
            if (!_state[i].Exists)
            {
                _state[i] = command == "yes";
                break;
            }
        }

        Select();
    }

    public override void Cancel() => Reset();
    public override void Reset() => _state = new Maybe<bool>[5];

    private static string Primary => Edgework.Strikes % 2 == 1 ? "right" : "left";
    private static string Secondary => Edgework.Strikes % 2 == 0 ? "right" : "left";

    private void Solution(string v)
    {
        Speak("Drink " + v);
        Reset();
        ExitSubmenu();
        Solve();
    }

    public override void Select()
    {
        if (!_registered)
        {
            OnStrike += Reset;
            _registered = true;
        }

        // 1
        if (!_state[0].Exists)
        {
            Speak("Is " + Primary + " silver or gold?");
            return;
        }

        if (_state[0].Item)
        {
            // 2
            if (!_state[1].Exists)
            {
                Speak("Does " + Primary + " have 8 or more gems?");
                return;
            }

            if (_state[1].Item)
            {
                // 3
                if (!_state[2].Exists)
                {
                    Speak("Are " + Primary + " gems white red or blue?");
                    return;
                }

                if (_state[2].Item)
                {
                    // 4
                    if (!_state[3].Exists)
                    {
                        Speak("Are both goblets the same material?");
                        return;
                    }

                    if (_state[3].Item)
                    {
                        // 5
                        if (!_state[4].Exists)
                        {
                            Speak("Is " + Primary + " liquid red or blue?");
                            return;
                        }

                        Solution(_state[4].Item ? Primary : Secondary);
                        return;
                    }
                    else
                    {
                        // 6
                        if (!_state[4].Exists)
                        {
                            Speak("Is " + Primary + " liquid green or orange?");
                            return;
                        }

                        Solution(_state[4].Item ? "right" : "left");
                        return;
                    }
                }
                else
                {
                    // 7
                    if (!_state[3].Exists)
                    {
                        Speak("Is " + Primary + " liquid purple or cyan?");
                        return;
                    }

                    if (_state[3].Item)
                    {
                        // 8
                        if (!_state[4].Exists)
                        {
                            Speak("Is the " + Primary + " liquid the same color as its gems?");
                            return;
                        }

                        Solution(_state[4].Item ? "left" : "right");
                        return;
                    }
                    else
                    {
                        // 9
                        if (!_state[4].Exists)
                        {
                            Speak("Are both goblets the same shape?");
                            return;
                        }

                        Solution(_state[4].Item ? Secondary : Primary);
                        return;
                    }
                }
            }
            else
            {
                // 10
                if (!_state[2].Exists)
                {
                    Speak("Is " + Primary + " shorter?");
                    return;
                }

                if (_state[2].Item)
                {
                    // 11
                    if (!_state[3].Exists)
                    {
                        Speak("Are " + Primary + " gems green yellow or purple?");
                        return;
                    }

                    if (_state[3].Item)
                    {
                        // 12
                        if (!_state[4].Exists)
                        {
                            Speak("Is " + Primary + " liquid red or orange?");
                            return;
                        }

                        Solution(_state[4].Item ? "right" : "left");
                        return;
                    }
                    else
                    {
                        // 13
                        if (!_state[4].Exists)
                        {
                            Speak("Is " + Primary + " liquid green or purple?");
                            return;
                        }

                        Solution(_state[4].Item ? Secondary : Primary);
                        return;
                    }
                }
                else
                {
                    // 14
                    if (!_state[3].Exists)
                    {
                        Speak("Does " + Primary + " have gems on bowl?");
                        return;
                    }

                    if (_state[3].Item)
                    {
                        // 15
                        if (!_state[4].Exists)
                        {
                            Speak("Are " + Primary + " gems white cyan or black?");
                            return;
                        }

                        Solution(_state[4].Item ? Primary : Secondary);
                        return;
                    }
                    else
                    {
                        // 16
                        if (!_state[4].Exists)
                        {
                            Speak("Do both goblets have same gem color?");
                            return;
                        }

                        Solution(_state[4].Item ? Primary : Secondary);
                        return;
                    }
                }
            }
        }
        else
        {
            // 17
            if (!_state[1].Exists)
            {
                Speak("Is " + Primary + " taller?");
                return;
            }

            if (_state[1].Item)
            {
                // 18
                if (!_state[2].Exists)
                {
                    Speak("Is " + Primary + " liquid blue or cyan?");
                    return;
                }

                if (_state[2].Item)
                {
                    // 19
                    if (!_state[3].Exists)
                    {
                        Speak("Does " + Primary + " have 7 or less gems?");
                        return;
                    }

                    if (_state[3].Item)
                    {
                        // 20
                        if (!_state[4].Exists)
                        {
                            Speak("Are " + Primary + " gems red green or black?");
                            return;
                        }

                        Solution(_state[4].Item ? "right" : "left");
                        return;
                    }
                    else
                    {
                        // 21
                        if (!_state[4].Exists)
                        {
                            Speak("Are " + Primary + " gems blue yellow or cyan?");
                            return;
                        }

                        Solution(_state[4].Item ? Primary : Secondary);
                        return;
                    }
                }
                else
                {
                    // 22
                    if (!_state[3].Exists)
                    {
                        Speak("Are " + Primary + " gems red purple or black?");
                        return;
                    }

                    if (_state[3].Item)
                    {
                        // 23
                        if (!_state[4].Exists)
                        {
                            Speak("Is " + Primary + " bronze?");
                            return;
                        }

                        Solution(_state[4].Item ? Secondary : Primary);
                        return;
                    }
                    else
                    {
                        // 24
                        if (!_state[4].Exists)
                        {
                            Speak("Is " + Primary + " iron?");
                            return;
                        }

                        Solution(_state[4].Item ? "right" : "left");
                        return;
                    }
                }
            }
            else
            {
                // 25
                if (!_state[2].Exists)
                {
                    Speak("Does " + Primary + " have gems on base?");
                    return;
                }

                if (_state[2].Item)
                {
                    // 26
                    if (!_state[3].Exists)
                    {
                        Speak("Are " + Primary + " gems blue green or purple?");
                        return;
                    }

                    if (_state[3].Item)
                    {
                        // 27
                        if (!_state[4].Exists)
                        {
                            Speak("Is " + Primary + " liquid red or purple?");
                            return;
                        }

                        Solution(_state[4].Item ? Secondary : Primary);
                        return;
                    }
                    else
                    {
                        // 28
                        if (!_state[4].Exists)
                        {
                            Speak("Is " + Primary + " liquid green or cyan?");
                            return;
                        }

                        Solution(_state[4].Item ? "left" : "right");
                        return;
                    }
                }
                else
                {
                    // 29
                    if (!_state[3].Exists)
                    {
                        Speak("Is " + Primary + " liquid blue or orange?");
                        return;
                    }

                    if (_state[3].Item)
                    {
                        // 30
                        if (!_state[4].Exists)
                        {
                            Speak("Are " + Primary + " gems white yellow or black?");
                            return;
                        }

                        Solution(_state[4].Item ? "left" : "right");
                        return;
                    }
                    else
                    {
                        // 31
                        if (!_state[4].Exists)
                        {
                            Speak("Are " + Primary + " gems red purple or cyan?");
                            return;
                        }

                        Solution(_state[4].Item ? Primary : Secondary);
                        return;
                    }
                }
            }
        }
    }
}
