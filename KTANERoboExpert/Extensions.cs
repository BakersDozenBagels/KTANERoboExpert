using System.Text;

namespace KTANERoboExpert;

internal static class Extensions
{
    public static string Join(this IEnumerable<string> en, string separator = "") =>
        en.Any()
        ? en.Aggregate((a, b) => a + separator + b)
        : "";

    public static void RemoveRangeQuietly<T>(this List<T> list, int start, int count)
    {
        if (start >= list.Count)
            return;
        list.RemoveRange(start, Math.Min(count, list.Count - start));
    }

    public static string Conjoin(this IEnumerable<string> strings, string sep = ", ", string lastSep = ", and ")
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
            sb.Append(sep).Append(prev);
            prev = en.Current;
        }
        return sb.Append(lastSep).Append(prev).ToString();
    }
}
