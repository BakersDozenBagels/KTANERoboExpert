namespace KTANERoboExpert.Uncertain;

public static class UncertainExtensions
{
    /// <summary>Maps this value to a new one if it is certain, or propogates uncertainty otherwise.</summary>
    public static IUncertain<U> Map<T, U>(this IUncertain<T> u, Func<T, U> map) where T : notnull where U : notnull => u.IsCertain ? map(u.Value!) : Uncertain<U>.Of(u.Fill);
    /// <summary>Maps this value to an uncertain value if it is certain, or propogates uncertainty otherwise.</summary>
    public static IUncertain<U> FlatMap<T, U>(this IUncertain<T> u, Func<T, IUncertain<U>> map) where T : notnull where U : notnull => u.IsCertain ? map(u.Value!) : Uncertain<U>.Of(u.Fill);
    /// <summary>Maps this value to an uncertain value if it is certain, or propogates uncertainty otherwise.</summary>
    public static IUncertain<T> FlatMap<T>(this IUncertain<IUncertain<T>> u) where T : notnull => u.IsCertain ? u.Value! : Uncertain<T>.Of(u.Fill);
    /// <summary>Gets the value if it is certain, or a default value if it is not.</summary>
    public static T OrElse<T>(this IUncertain<T> u, T value) where T : notnull => u.IsCertain ? u.Value! : value;
    /// <summary>Gets the value if it is certain, or a default value provided by a function if it is not.</summary>
    public static T OrElse<T>(this IUncertain<T> u, Func<T> value) where T : notnull => u.IsCertain ? u.Value! : value();

    /// <summary>Maps the value if it is certain, or gets a default value if it is not.</summary>
    public static U Match<T, U>(this IUncertain<T> u, Func<T, U> map, Func<U> orElse) where T : notnull where U : notnull => u.IsCertain ? map(u.Value!) : orElse();
    /// <summary>Maps the value if it is certain, or gets a default value provided by a function if it is not.</summary>
    public static U Match<T, U>(this IUncertain<T> u, Func<T, U> map, U orElse) where T : notnull where U : notnull => u.IsCertain ? map(u.Value!) : orElse;

    /// <summary>Unpacks and performs an action depending on the certainty.</summary>
    public static void Do<T>(this IUncertain<T> u, Action<IUncertain<T>> onUncertain, Action<T> onCertain) where T : notnull
    {
        if (u.IsCertain)
            onCertain(u.Value!);
        else
            onUncertain(u);
    }

    /// <summary>Gets the letters in the serial number.</summary>
    public static UncertainEnumerable<char> SerialNumberLetters(this Edgework edgework) =>
        edgework.SerialNumber.Match(
            s => UncertainEnumerable<char>.Of(s.Where(c => !"0123456789".Contains(c))),
            () => UncertainEnumerable<char>.Of(edgework.SerialNumber.Fill, 2, 4));
    /// <summary>Gets the vowels in the serial number. This does not include Y.</summary>
    public static UncertainEnumerable<char> SerialNumberVowels(this Edgework edgework) =>
        edgework.SerialNumberLetters().Where("AEIOU".Contains);
    /// <summary>Gets the consonants in the serial number. This does include Y.</summary>
    public static UncertainEnumerable<char> SerialNumberConsonants(this Edgework edgework) =>
        edgework.SerialNumberLetters().Where(c => !"AEIOU".Contains(c));
    /// <summary>Gets the numeric digits in the serial number.</summary>
    public static UncertainEnumerable<int> SerialNumberDigits(this Edgework edgework) =>
        edgework.SerialNumber.Match(
            s => UncertainEnumerable<int>.Of(s.Where("0123456789".Contains).Select(c => int.Parse(c.ToString()))),
            () => UncertainEnumerable<int>.Of(edgework.SerialNumber.Fill, 2, 4));
    /// <summary>Tests whether the bomb has an indicator with the given properties.</summary>
    /// <param name="label">Optionally, the label to check for.</param>
    /// <param name="lit">Optionally, whether the indicator should be lit or unlit.</param>
    public static UncertainBool HasIndicator(this Edgework edgework, Maybe<string> label = default, Maybe<bool> lit = default) =>
        edgework.Indicators.Match(
            v => v.Any(i => (!label.Exists || i.Label == label.Item) && (!lit.Exists || i.Lit == lit.Item)),
            () => UncertainBool.Of(edgework.Indicators.Fill));
    /// <summary>Tests whether the bomb has an indicator with any of the provided labels.</summary>
    public static UncertainBool HasAnyIndicator(this Edgework edgework, params IEnumerable<string> labels) =>
        labels.Select(l => edgework.HasIndicator(l)).Aggregate((a, b) => a | b);

    /// <summary>Upcasts this value, while optionally enhancing it with a known minimum and maximum.</summary>
    public static UncertainInt Into(this IUncertain<int> i, Maybe<int> min = default, Maybe<int> max = default) =>
        i.IsCertain ? i.Value : UncertainInt.InRange(min, max, i.Fill);
    /// <summary>Upcasts this value.</summary>
    public static UncertainBool Into(this IUncertain<bool> b) => b.IsCertain ? UncertainBool.Of(b.Value) : UncertainBool.Of(b.Fill);
    /// <summary>Upcasts this value.</summary>
    public static UncertainEnumerable<T> Into<T>(this IUncertain<IEnumerable<T>> b) where T : notnull => b.IsCertain ? UncertainEnumerable<T>.Of(b.Value!) : UncertainEnumerable<T>.Of(b.Fill);

    /// <summary>Collapses two ranges into one.</summary>
    public static UncertainInt Coalesce(this UncertainInt u, UncertainInt other) => u.ButWithinRange(other.Min, other.Max);

    /// <summary>Filters the sequence based on a predicate.</summary>
    public static UncertainEnumerable<T> Where<T>(this IEnumerable<T> en, Func<T, IUncertain<bool>> predicate) where T : notnull => en.Where((e, i) => predicate(e));
    /// <inheritdoc cref="Where{T}(IEnumerable{T}, Func{T, IUncertain{bool}})"/>
    public static UncertainEnumerable<T> Where<T>(this IEnumerable<T> en, Func<T, int, IUncertain<bool>> predicate) where T : notnull
    {
        if (en.Select(predicate).All(x => x.IsCertain))
            return UncertainEnumerable<T>.Of(en.Where((e, i) => predicate(e, i).Value!));

        var counts = en.Select((e, i) => predicate(e, i) switch { { IsCertain: true, Value: true } => (1, 1), { IsCertain: true, Value: false } => (0, 0), { IsCertain: false } => (0, 1) }).Aggregate((a, b) => (a.Item1 + b.Item1, a.Item2 + b.Item2));

        return UncertainEnumerable<T>.Of(en.Select(predicate).First(t => !t.IsCertain).Fill, counts.Item1, counts.Item2);
    }

    /// <summary>Counts the number of items potentially in this sequence.</summary>
    public static UncertainInt Count<T>(this UncertainEnumerable<T> en) where T : notnull => en.Count;
    /// <summary>Counts the number of items potentially in this sequence that match a given criterion.</summary>
    public static UncertainInt Count<T>(this UncertainEnumerable<T> en, Func<T, bool> predicate) where T : notnull => en.Where(predicate).Count;
    /// <summary>Counts the number of items potentially in this sequence that match a given criterion.</summary>
    public static UncertainInt Count<T>(this UncertainEnumerable<T> en, Func<T, int, bool> predicate) where T : notnull => en.Where(predicate).Count;
    /// <summary>Counts the number of items potentially in this sequence that potentially match a given criterion.</summary>
    public static UncertainInt Count<T>(this UncertainEnumerable<T> en, Func<T, IUncertain<bool>> predicate) where T : notnull => en.Where(predicate).Count;
    /// <summary>Counts the number of items potentially in this sequence that potentially match a given criterion.</summary>
    public static UncertainInt Count<T>(this UncertainEnumerable<T> en, Func<T, int, IUncertain<bool>> predicate) where T : notnull => en.Where(predicate).Count;

    /// <summary>Tests whether this sequence contains a given item.</summary>
    public static UncertainBool Contains<T>(this UncertainEnumerable<T> en, T item) where T : notnull => en.Count(x => EqualityComparer<T>.Default.Equals(x, item)) > 0;

    /// <summary>Tests if this number is odd.</summary>
    public static UncertainBool IsOdd(this IUncertain<int> u) => u.Map(v => v % 2 is 1).Into();
    /// <summary>Tests if this number is even.</summary>
    public static UncertainBool IsEven(this IUncertain<int> u) => u.Map(v => v % 2 is 0).Into();
}
