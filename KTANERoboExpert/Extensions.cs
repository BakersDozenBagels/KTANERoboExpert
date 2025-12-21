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
    /// Expands a new list to contain at least <paramref name="count"/> items, filling any new ones with a default value.
    /// </summary>
    public static List<T> SparseExpanded<T>(this IEnumerable<T> l, int count, Func<T> @default) => l.SparseExpanded(count, i => @default());
    /// <summary>
    /// Expands a new list to contain at least <paramref name="count"/> items, filling any new ones with a default value.
    /// </summary>
    public static List<T> SparseExpanded<T>(this IEnumerable<T> l, int count, Func<int, T> @default)
    {
        var lc = l.ToList();
        lc.SparseExpand(count, @default);
        return lc;
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

    public static IEnumerable<int> AllIndicesOf<T>(this T[] en, T item)
    {
        int ix = 0;
        ix = Array.IndexOf(en, item, ix);
        while (ix is not -1)
        {
            yield return ix;
            ix = Array.IndexOf(en, item, ix + 1);
        }
    }
}
