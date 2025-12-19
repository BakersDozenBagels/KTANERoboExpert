using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class BoneAppleTea : RoboExpertModule
{
    public override string Name => "Bone Apple Tea";
    public override string Help => "seizure salad";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices(_words));

    private static readonly string[] _words = ["Bone Apple Tea", "Seizure Salad", "Hey to break it to ya", "This is oak ward", "Clea Shay", "It's in tents", "Bench watch", "You're an armature", "Man hat in", "Try all and era", "Million Air", "Die of beaties", "Rush and roulette", "Night and shining armour", "What a nice jester", "In some near", "This is my master peace", "I'm in a colder sac", "Cereal killer", "I come here off ten", "Slide of ham", "Test lah", "Refreshing campaign", "I'm being more pacific", "God blast you", "BC soft wear", "Sense in humor", "The three must of tears", "Third da men chin", "Prang mantas", "Hammy downs", "Yum, a case idea", "Dandy long legs", "Can't merge, little lone drive", "My guest is", "Sink", "You lake it", "Emit da feet"];
    private static readonly string[] _answers = [.. Enumerable.Range(0, 10).Select(i => i.ToString()), .. NATO, "ampersand", "dollar"];

    private int _done = 0;

    public override void ProcessCommand(string command)
    {
        Speak(_answers[_words.IndexOf(command)]);

        _done++;

        if (_done is 2)
        {
            _done = 0;
            ExitSubmenu();
            Solve();
        }
    }

    public override void Cancel() => Reset();
    public override void Reset() => _done = 0;
}
