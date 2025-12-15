using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace KTANERoboExpert;

/// <summary>
/// Represents an item which may or may not be present.
/// </summary>
/// <typeparam name="T">The type of item</typeparam>
public readonly struct Maybe<T> : IEnumerable<T>, IOrderedEnumerable<T>, IEquatable<Maybe<T>>
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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IOrderedEnumerable<T> IOrderedEnumerable<T>.CreateOrderedEnumerable<TKey>(
        Func<T, TKey> keySelector,
        IComparer<TKey>? comparer,
        bool descending
    ) => this;

    public static implicit operator Maybe<T>(T item) => new(item);

    public Maybe<U> Map<U>(Func<T, U> map) => Exists ? map(Item) : new Maybe<U>();
    public Maybe<U> FlatMap<U>(Func<T, Maybe<U>> map) => Exists ? map(Item) : new Maybe<U>();

    public T OrElse(T value) => Exists ? Item : value;

    public override bool Equals(object? obj) => obj is Maybe<T> maybe && Equals(maybe);
    public bool Equals(Maybe<T> other) => Exists == other.Exists && EqualityComparer<T?>.Default.Equals(Item, other.Item);
    public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);
    public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);
    public override int GetHashCode() => HashCode.Combine(Exists, Item);
}
