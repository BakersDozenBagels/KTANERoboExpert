namespace KTANERoboExpert.Uncertain;

/// <summary>Represents a sequence of values dependent on edgework that may or may not be known.</summary>
public class UncertainEnumerable<T> : Uncertain<IEnumerable<T>> where T : notnull
{
    private readonly Maybe<int> _minLength = new(), _maxLength = new();

    private UncertainEnumerable(IEnumerable<T> value) : base(value) { }
    private UncertainEnumerable(Action<Action, Action?> fill, Maybe<int> minLength = default, Maybe<int> maxLength = default) : base(fill)
    {
        _minLength = minLength;
        _maxLength = maxLength;
    }

    /// <summary>A definitely known sequence.</summary>
    public static new UncertainEnumerable<T> Of(IEnumerable<T> value) => new(value);

    /// <summary>A definitely unknown sequence.</summary>
    public static UncertainEnumerable<T> Of(Action<Action, Action?> fill, Maybe<int> minLength = default, Maybe<int> maxLength = default)
    {
        if (minLength.Exists && maxLength.Exists && minLength.Item is 0 && maxLength.Item is 0)
            return new([]);
        return new(fill, minLength, maxLength);
    }

    /// <summary>The number of items in this sequence.</summary>
    public UncertainInt Count => IsCertain ? Value.Count() : Range();

    private UncertainInt Range() =>
        (_minLength.Exists, _maxLength.Exists) switch
        {
            (true, true) => UncertainInt.InRange(_minLength.Item, _maxLength.Item, Fill),
            (true, false) => UncertainInt.AtLeast(_minLength.Item, Fill),
            (false, true) => UncertainInt.InRange(0, _maxLength.Item, Fill),
            (false, false) => UncertainInt.Unknown(Fill)
        };

    /// <summary>The element at the supplied index, if known.</summary>
    public Uncertain<T> this[Index ix] => IsCertain ? Value.ElementAt(ix) : Uncertain<T>.Of(Fill);

    /// <summary>Filters the sequence based on a predicate.</summary>
    public UncertainEnumerable<T> Where(Func<T, bool> predicate) => Where((e, i) => predicate(e));
    /// <summary>Filters the sequence based on a predicate.</summary>
    public UncertainEnumerable<T> Where(Func<T, int, bool> predicate) => IsCertain ? Of(Value.Where(predicate)) : Of(_getValue.Item!, 0, _maxLength);
    /// <summary>Filters the sequence based on a predicate.</summary>
    public UncertainEnumerable<T> Where(Func<T, IUncertain<bool>> predicate) => Where((e, i) => predicate(e));
    /// <summary>Filters the sequence based on a predicate.</summary>
    public UncertainEnumerable<T> Where(Func<T, int, IUncertain<bool>> predicate) => IsCertain ? Value.Where(predicate) : Of(_getValue.Item!, 0, _maxLength);

    /// <summary>Projects each element of the sequence to a new form.</summary>
    public UncertainEnumerable<U> Select<U>(Func<T, U> selector) where U : notnull => Select((e, i) => selector(e));
    /// <summary>Projects each element of the sequence to a new form.</summary>
    public UncertainEnumerable<U> Select<U>(Func<T, int, U> selector) where U : notnull => IsCertain ? UncertainEnumerable<U>.Of(Value.Select(selector)) : UncertainEnumerable<U>.Of(_getValue.Item!, _minLength, _maxLength);

    /// <summary>Provides a lower bound for <see cref="Count"/> to make deductions when the value is uncertain.</summary>
    public UncertainEnumerable<T> ButAtLeast(int min) => Of(Fill, _minLength.Map(x => Math.Max(x, min)).OrElse(min), _maxLength);
    /// <summary>Provides an upper bound for <see cref="Count"/> to make deductions when the value is uncertain.</summary>
    public UncertainEnumerable<T> ButAtMost(int max) => Of(Fill, _minLength, _maxLength.Map(x => Math.Min(x, max)).OrElse(max));
    /// <summary>Provides a lower and upper bound for <see cref="Count"/> to make deductions when the value is uncertain.</summary>
    public UncertainEnumerable<T> ButWithinRange(int min, int max) => Of(Fill, _minLength.Map(x => Math.Max(x, min)).OrElse(min), _maxLength.Map(x => Math.Min(x, max)).OrElse(max));
}
