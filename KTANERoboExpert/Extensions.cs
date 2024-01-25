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
}
