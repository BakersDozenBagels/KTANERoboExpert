using System.Collections;

namespace KTANERoboExpert.Uncertain;

public class UncertainEnumerable<T> : Uncertain<IEnumerable<T>>, IEnumerable<T>
{
    private readonly Maybe<int> _minLength = new(), _maxLength = new();

    private UncertainEnumerable(IEnumerable<T> value) : base(value) { }
    private UncertainEnumerable(Action<Action, Action?> fill, Maybe<int> minLength = default, Maybe<int> maxLength = default) : base(fill)
    {
        _minLength = minLength;
        _maxLength = maxLength;
    }

    public static UncertainEnumerable<T> Of(IEnumerable<T> value) => new(value);
    public static UncertainEnumerable<T> Of(Action<Action, Action?> fill, Maybe<int> minLength = default, Maybe<int> maxLength = default)
    {
        if (minLength.Exists && maxLength.Exists && minLength.Item == maxLength.Item)
            return new([]);
        return new(fill, minLength, maxLength);
    }

    public UncertainInt Count => IsCertain ? Value.Count() : OfRange();

    private UncertainInt OfRange()
    {
        return (_minLength.Exists, _maxLength.Exists) switch
        {
            (true, true) => UncertainInt.InRange(_minLength.Item, _maxLength.Item, Fill),
            (true, false) => UncertainInt.AtLeast(_minLength.Item, Fill),
            (false, true) => UncertainInt.InRange(0, _maxLength.Item, Fill),
            (false, false) => new UncertainInt(Fill)
        };
    }

    public IEnumerator<T> GetEnumerator() => Value!.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value!).GetEnumerator();

    public UncertainEnumerable<T> Where(Func<T, bool> predicate) => Where((e, i) => predicate(e));
    public UncertainEnumerable<T> Where(Func<T, int, bool> predicate) => IsCertain ? Of(Value.Where(predicate)) : Of(_getValue.Item!, 0, _maxLength);

    public UncertainEnumerable<U> Select<U>(Func<T, U> selector) => Select((e, i) => selector(e));
    public UncertainEnumerable<U> Select<U>(Func<T, int, U> selector) => IsCertain ? UncertainEnumerable<U>.Of(Value.Select(selector)) : UncertainEnumerable<U>.Of(_getValue.Item!, _minLength, _maxLength);
}
