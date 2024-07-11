using System.Speech.Recognition;
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
    public static void RemoveRangeQuietly<T>(this List<T> list, int start, int count = int.MaxValue)
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
    /// Calls <see cref="GrammarBuilder.Append(GrammarBuilder)"/> and returns <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder to append to</param>
    /// <param name="other">The builder to be appended</param>
    /// <returns><paramref name="builder"/></returns>
    public static GrammarBuilder Then(this GrammarBuilder builder, GrammarBuilder other)
    {
        builder.Append(other);
        return builder;
    }

    /// <summary>
    /// Tests whether the serial number has a vowel. This does not include Y.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown when the serial number is unknown.</exception>
    /// <param name="edgework">The edgework to test</param>
    /// <returns><see langword="true"/> if the serial number has a vowel, <see cref="false"/> otherwise.</returns>
    public static bool HasSerialNumberVowel(this Edgework edgework) => edgework.SerialNumber!.Any("AEIOU".Contains);
}
