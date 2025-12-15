using System.Diagnostics;
using System.Speech.Recognition;

namespace KTANERoboExpert.Modules;

public class Maze : RoboExpertModule
{
    public override string Name => "Maze";
    public override string Help => "circle A1 circle B2 square C3 triangle D4";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("circle", "square", "triangle").ToGrammarBuilder() + new Choices(NATO.Take(6).ToArray()) + new Choices("1", "2", "3", "4", "5", "6"), 1, 4));

    private MazeData? _data;

    public override void ProcessCommand(string command)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 && parts.Length != 12)
        {
            Speak("Pardon?");
            return;
        }

        if (parts.Length == 12)
        {
            var chunks = parts
                .Chunk(3)
                .Select(c => (Command: c[0], Column: c[1][0], Row: int.Parse(c[2])))
                .OrderBy(c => c.Command)
                .ThenBy(c => c.Row)
                .ThenBy(c => c.Column)
                .ToArray();
            if (!chunks.Select(c => c.Command).SequenceEqual(["circle", "circle", "square", "triangle"]))
            {
                Speak("Pardon?");
                return;
            }

            int? mazeIx = (chunks[0].Column, chunks[0].Row, chunks[1].Column, chunks[1].Row) switch
            {
                ('a', 2, 'f', 3) => 0,
                ('e', 2, 'b', 4) => 1,
                ('d', 4, 'f', 4) => 2,
                ('a', 1, 'a', 4) => 3,
                ('e', 3, 'd', 6) => 4,
                ('e', 1, 'c', 5) => 5,
                ('b', 1, 'b', 6) => 6,
                ('d', 1, 'c', 4) => 7,
                ('c', 2, 'a', 5) => 8,
                _ => null
            };
            if (mazeIx is not { } mz)
            {
                Speak("Pardon?");
                return;
            }

            _data = new(mz, "abcdef".IndexOf(chunks[3].Column), chunks[3].Row - 1, false);

            parts = [chunks[2].Command, chunks[2].Column.ToString(), chunks[2].Row.ToString()];
        }

        Debug.Assert(parts.Length == 3);
        if (_data == null || parts[0] != "square")
        {
            Speak("Pardon?");
            return;
        }

        var col = "abcdef".IndexOf(parts[1][0]);
        var row = int.Parse(parts[2]) - 1;

        Queue<(byte Row, byte Col, string Path)> todo = new([((byte)row, (byte)col, "")]);
        HashSet<(byte Row, byte Col)> done = [];

        var data = _data.Value;
        var maze = _mazes[data.Maze];

        string? path = null;

        static byte Dec(byte b) => --b;
        static byte Inc(byte b) => ++b;

        while (todo.TryDequeue(out var obj))
        {
            if (obj.Row == data.Row && obj.Col == data.Col)
            {
                path = obj.Path;
                break;
            }
            done.Add((obj.Row, obj.Col));
            var cell = maze[obj.Row][obj.Col];

            if ((cell & 1) == 1 && !done.Contains((Dec(obj.Row), obj.Col)))
                todo.Enqueue((Dec(obj.Row), obj.Col, obj.Path + " Up"));
            if ((cell & 2) == 2 && !done.Contains((Inc(obj.Row), obj.Col)))
                todo.Enqueue((Inc(obj.Row), obj.Col, obj.Path + " Down"));
            if ((cell & 4) == 4 && !done.Contains((obj.Row, Dec(obj.Col))))
                todo.Enqueue((obj.Row, Dec(obj.Col), obj.Path + " Left"));
            if ((cell & 8) == 8 && !done.Contains((obj.Row, Inc(obj.Col))))
                todo.Enqueue((obj.Row, Inc(obj.Col), obj.Path + " Right"));
        }

        Debug.Assert(path is not null);
        Speak(path[1..]);
    }

    public override void Reset() => _data = null;

    public override void Cancel()
    {
        if (_data is { Solved: false } d)
        {
            Load(() => Solve());
            _data = d with { Solved = true };
        }
    }

    private readonly record struct MazeData(int Maze, int Col, int Row, bool Solved);

    /// <summary>
    /// up open = 1
    /// down open = 2
    /// left open = 4
    /// right open = 8
    /// </summary>
    private static readonly byte[][][] _mazes =
    [
        [ // 0
            [0xa, 0xc, 0x6, 0xa, 0xc, 0x4],
            [0x3, 0xa, 0x5, 0x9, 0xc, 0x6],
            [0x3, 0x9, 0x6, 0xa, 0xc, 0x7],
            [0x3, 0x8, 0xd, 0x5, 0x8, 0x7],
            [0xb, 0xc, 0x6, 0xa, 0x4, 0x3],
            [0x9, 0x4, 0x9, 0x5, 0x8, 0x5],
        ],
        [ // 1
            [0x8, 0xe, 0x4, 0xa, 0xe, 0x4],
            [0xa, 0x5, 0xa, 0x5, 0x9, 0x6],
            [0x3, 0xa, 0x5, 0xa, 0xc, 0x7],
            [0xb, 0x5, 0xa, 0x5, 0x2, 0x3],
            [0x3, 0x2, 0x3, 0xa, 0x5, 0x3],
            [0x1, 0x9, 0x5, 0x9, 0xc, 0x5],
        ],
        [ // 2
            [0xa, 0xc, 0x6, 0x2, 0xa, 0x6],
            [0x1, 0x2, 0x3, 0x9, 0x5, 0x3],
            [0xa, 0x7, 0x3, 0xa, 0x6, 0x3],
            [0x3, 0x3, 0x3, 0x3, 0x3, 0x3],
            [0x3, 0x9, 0x5, 0x3, 0x3, 0x3],
            [0x9, 0xc, 0xc, 0x5, 0x9, 0x5],
        ],
        [ // 3
            [0xa, 0x6, 0x8, 0xc, 0xc, 0x6],
            [0x3, 0x3, 0xa, 0xc, 0xc, 0x7],
            [0x3, 0x9, 0x5, 0xa, 0x4, 0x3],
            [0x3, 0x8, 0xc, 0xd, 0xc, 0x7],
            [0xb, 0xc, 0xc, 0xc, 0x6, 0x3],
            [0x9, 0xc, 0x4, 0x8, 0x5, 0x1],
        ],
        [ // 4
            [0x8, 0xc, 0xc, 0xc, 0xe, 0x6],
            [0xa, 0xc, 0xc, 0xe, 0x5, 0x1],
            [0xb, 0x6, 0x8, 0x5, 0xa, 0x6],
            [0x3, 0x9, 0xc, 0x6, 0x1, 0x3],
            [0x3, 0xa, 0xc, 0xd, 0x4, 0x3],
            [0x1, 0x9, 0xc, 0xc, 0xc, 0x5],
        ],
        [ // 5
            [0x2, 0xa, 0x6, 0x8, 0xe, 0x6],
            [0x3, 0x3, 0x3, 0xa, 0x5, 0x3],
            [0xb, 0x5, 0x1, 0x3, 0xa, 0x5],
            [0x9, 0x6, 0xa, 0x7, 0x3, 0x2],
            [0xa, 0x5, 0x1, 0x3, 0x9, 0x7],
            [0x9, 0xc, 0xc, 0x5, 0x8, 0x5],
        ],
        [ // 6
            [0xa, 0xc, 0xc, 0x6, 0xa, 0x6],
            [0x3, 0xa, 0x4, 0x9, 0x5, 0x3],
            [0x9, 0x5, 0xa, 0x4, 0xa, 0x5],
            [0xa, 0x6, 0xb, 0xc, 0x5, 0x2],
            [0x3, 0x1, 0x9, 0xc, 0x6, 0x3],
            [0x9, 0xc, 0xc, 0xc, 0xd, 0x5],
        ],
        [ // 7
            [0x2, 0xa, 0xc, 0x6, 0xa, 0x6],
            [0xb, 0xd, 0x4, 0x9, 0x5, 0x3],
            [0x3, 0xa, 0xc, 0xc, 0x6, 0x3],
            [0x3, 0x9, 0x6, 0x8, 0xd, 0x5],
            [0x3, 0x2, 0x9, 0xc, 0xc, 0x4],
            [0x9, 0xd, 0xc, 0xc, 0xc, 0x4],
        ],
        [ // 8
            [0x2, 0xa, 0xc, 0xc, 0xe, 0x6],
            [0x3, 0x3, 0xa, 0x4, 0x3, 0x3],
            [0xb, 0xd, 0x5, 0xa, 0x5, 0x3],
            [0x3, 0x2, 0xa, 0x5, 0x8, 0x7],
            [0x3, 0x3, 0x3, 0xa, 0x6, 0x1],
            [0x9, 0x5, 0x9, 0x5, 0x9, 0x4],
        ]
    ];
}
