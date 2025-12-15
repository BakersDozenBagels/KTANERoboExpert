using KTANERoboExpert.Uncertain;
using System.Text;

namespace KTANERoboExpert;

internal static class Extensions
{
    /// <summary>
    /// Removes elements from a list while silently ignoring elements out of range.
    /// </summary>
    /// <typeparam name="T">The list element type</typeparam>
    /// <param name="list">The list to remove from</param>
    /// <param name="start">The first element to remove</param>
    /// <param name="count">The number of elements to remove</param>
    public static void GuardedRemoveRange<T>(this List<T> list, int start, int count = int.MaxValue)
    {
        if (start >= list.Count)
            return;
        start = Math.Max(start, 0);
        list.RemoveRange(start, Math.Clamp(count, 0, list.Count - start));
    }

    /// <summary>
    /// Joins strings together, optionally with a different final separator.
    /// </summary>
    /// <param name="strings">The strings to join</param>
    /// <param name="separator">The separator to use</param>
    /// <param name="lastSeparator">The final separator to use</param>
    /// <returns>The joined strings</returns>
    public static string Conjoin(this IEnumerable<string> strings, string separator = " ", string? lastSeparator = null)
    {
        using var en = strings.GetEnumerator();
        if (!en.MoveNext())
            return string.Empty;
        string prev = en.Current;
        if (!en.MoveNext())
            return prev;
        StringBuilder sb = new();
        sb.Append(prev);
        prev = en.Current;
        while (en.MoveNext())
        {
            sb.Append(separator).Append(prev);
            prev = en.Current;
        }
        return sb.Append(lastSeparator ?? separator).Append(prev).ToString();
    }

    /// <summary>
    /// Expands a list to contain at least <paramref name="count"/> items, filling any new ones with a default value.
    /// </summary>
    public static void SparseExpand<T>(this List<T> l, int count, Func<T> @default) => l.SparseExpand(count, i => @default());
    /// <summary>
    /// Expands a list to contain at least <paramref name="count"/> items, filling any new ones with a default value.
    /// </summary>
    public static void SparseExpand<T>(this List<T> l, int count, Func<int, T> @default)
    {
        for (int i = l.Count; i < count; i++)
            l.Add(@default(i));
    }
    /// <summary>
    /// Sets a list item while filling any missing indices with a default value.
    /// </summary>
    public static void SparseSet<T>(this List<T> l, int index, T value, Func<T> @default) => l.SparseSet(index, value, i => @default());
    /// <summary>
    /// Sets a list item while filling any missing indices with a default value.
    /// </summary>
    public static void SparseSet<T>(this List<T> l, int index, T value, Func<int, T> @default)
    {
        for (int i = l.Count; i < index; i++)
            l.Add(@default(i));
        if (l.Count > index)
            l[index] = value;
        else
            l.Add(value);
    }

    /// <summary>
    /// Maps this value to value to an <see cref="UncertainBool"/> if it is certain, or propogates uncertainty otherwise.
    /// </summary>
    public static UncertainBool Matches<T>(this IUncertain<T> u, Func<T, UncertainBool> predicate) =>
        u.IsCertain ? predicate(u.Value!) : new(u.Fill);

    public static IUncertain<U> Map<T, U>(this IUncertain<T> u, Func<T, U> map) => u.IsCertain ? map(u.Value!) : new Uncertain<U>(u.Fill);
    public static IUncertain<U> FlatMap<T, U>(this IUncertain<T> u, Func<T, IUncertain<U>> map) => u.IsCertain ? map(u.Value!) : new Uncertain<U>(u.Fill);
    public static T OrElse<T>(this IUncertain<T> u, T value) => u.IsCertain ? u.Value! : value;

    /// <summary>
    /// Gets the letters in the serial number.
    /// </summary>
    public static UncertainEnumerable<char> SerialNumberLetters(this Edgework edgework) =>
        edgework.SerialNumber.Map(s => new UncertainEnumerable<char>(s.Where(c => !"0123456789".Contains(c)))).OrElse(new(edgework.SerialNumber.Fill, 2, 4));
    /// <summary>
    /// Gets the vowels in the serial number. This does not include Y.
    /// </summary>
    public static UncertainEnumerable<char> SerialNumberVowels(this Edgework edgework) =>
        edgework.SerialNumberLetters().Where("AEIOU".Contains);
    /// <summary>
    /// Gets the consonants in the serial number. This does include Y.
    /// </summary>
    public static UncertainEnumerable<char> SerialNumberConsonants(this Edgework edgework) =>
        edgework.SerialNumberLetters().Where(c => !"AEIOU".Contains(c));
    /// <summary>
    /// Gets the numeric digits in the serial number.
    /// </summary>
    public static UncertainEnumerable<int> SerialNumberDigits(this Edgework edgework) =>
        edgework.SerialNumber.Map(s => new UncertainEnumerable<int>(s.Where("0123456789".Contains).Select(c => int.Parse(c.ToString())))).OrElse(new(edgework.SerialNumber.Fill, 2, 4));
    /// <summary>
    /// Tests whether the bomb has an indicator with the given properties.
    /// </summary>
    /// <param name="label">Optionally, the label to check for.</param>
    /// <param name="lit">Optionally, whether the indicator should be lit or unlit.</param>
    public static UncertainBool HasIndicator(this Edgework edgework, Maybe<string> label = default, Maybe<bool> lit = default) =>
        edgework.Indicators.IsCertain
            ? new(edgework.Indicators.Value.Any(i => (!label.Exists || i.Label == label.Item) && (!lit.Exists || i.Lit == lit.Item)))
            : new(edgework.Indicators.Fill);
}
