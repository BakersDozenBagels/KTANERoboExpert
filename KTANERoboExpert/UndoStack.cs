using System.Diagnostics;

namespace KTANERoboExpert;

/// <summary>
/// Represents an undo stack spanning multiple modules of one type.
/// </summary>
/// <typeparam name="T">The type of state to store</typeparam>
/// <param name="baseState">The default state when no user input has been given</param>
public sealed class UndoStack<T>(T baseState) where T : notnull
{
    private int _pointer;
    private readonly List<HistoryNode> _history = [new(baseState, true)];
    private readonly T _baseState = baseState;

    /// <summary>
    /// The current state on top of the stack.
    /// </summary>
    public T Current
    {
        get => _history[_pointer].Frame;
        set => Do(value);
    }
    /// <summary>
    /// Performs a new action. This forgets any undone actions.
    /// </summary>
    /// <param name="item">The new state</param>
    public void Do(T item) => AddItem(new(item, false));
    /// <summary>
    /// Starts a new module instance.
    /// <see cref="Reset"/> will return here. This can be undone past.
    /// </summary>
    public void NewModule() => AddItem(new(_baseState, true));
    /// <summary>
    /// Starts a new module instance with the given starting state.
    /// <see cref="Reset"/> will return here. This can be undone past.
    /// </summary>
    public void NewModule(T item) => AddItem(new(item, true));
    private void AddItem(HistoryNode item)
    {
        _history.GuardedRemoveRange(_pointer + 1);
        _history.Add(item);
        _pointer++;
    }
    /// <summary>
    /// Undoes the most recent action.
    /// </summary>
    /// <returns>The state undone to, or an empty <see cref="Maybe{T}"/> if no undo is possible (i.e. when no actions have been taken).</returns>
    public Maybe<T> Undo() => _pointer == 0 ? new() : new(_history[--_pointer].Frame);
    /// <summary>
    /// Redoes the most recently undone action.
    /// </summary>
    /// <returns>The state redone to, or an empty <see cref="Maybe{T}"/> if no redo is possible (i.e. when no actions have been undone, or all undone actions have been forgotten).</returns>
    public Maybe<T> Redo() => _pointer == _history.Count - 1 ? new() : new(_history[++_pointer].Frame);
    /// <summary>
    /// Resets to the start of the current module instance. This forgets any undone actions. For a full reset, use <see cref="Clear"/>.
    /// </summary>
    /// <returns>The state reset to, or an empty <see cref="Maybe{T}"/> if no reset is possible (i.e. when already at the start of a module instance).</returns>
    public Maybe<T> Reset()
    {
        if (_history[_pointer].Reset)
            return new();
        for (int i = _pointer - 1; i >= 0; i--)
        {
            if (_history[i].Reset)
            {
                AddItem(_history[i]);
                return new(_history[i].Frame);
            }
        }

        throw new UnreachableException();
    }
    /// <summary>
    /// Fully resets the stack. For a module instance reset, use <see cref="Reset"/>.
    /// </summary>
    public void Clear()
    {
        _history.Clear();
        _history.Add(new(_baseState, true));
        _pointer = 0;
    }

    private readonly record struct HistoryNode(T Frame, bool Reset);
}