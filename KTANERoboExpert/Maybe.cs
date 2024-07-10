using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace KTANERoboExpert;

/// <summary>
/// Represents an item which may or may not be present.
/// </summary>
/// <typeparam name="T">The type of item</typeparam>
public readonly struct Maybe<T> : IEnumerable<T>
{
    /// <summary>
    /// <see langword="true"/> when the item exists, <see langword="false"/> otherwise.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Item))]
    public bool Exists { get; }
    /// <summary>
    /// The item.
    /// </summary>
    public T? Item { get; }

    /// <summary>
    /// Creates a new instance with an item.
    /// </summary>
    /// <param name="item">The item</param>
    public Maybe(T item)
    {
        Exists = true;
        Item = item;
    }
    /// <summary>
    /// Creates a new instance without an item.
    /// </summary>
    public Maybe() { }

    public IEnumerator<T> GetEnumerator()
    {
        if (Exists)
            yield return Item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        if (Exists)
            yield return Item;
    }
}
