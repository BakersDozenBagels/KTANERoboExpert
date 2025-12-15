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

    /// <summary>
    /// Tests whether the serial number has a vowel. This does not include Y.
    /// </summary>
    public static UncertainBool HasSerialNumberVowel(this Edgework edgework) => edgework.SerialNumber.Matches(s => s.Any("AEIOU".Contains));
    /// <summary>
    /// Gets the numeric digits in the serial number.
    /// </summary>
    public static Uncertain<IEnumerable<int>> SerialNumberDigits(this Edgework edgework) =>
        edgework.SerialNumber.IsCertain ? new(edgework.SerialNumber.Value.Where("0123456789".Contains).Select(c => int.Parse(c.ToString()))) : new(edgework.SerialNumber.Fill);
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
