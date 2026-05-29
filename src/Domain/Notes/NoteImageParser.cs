using System.Text.RegularExpressions;

namespace Domain.Notes;

public static class NoteImageParser
{
    private static readonly Regex ImagePattern = new(@"!\[[^\]]*\]\(([^)]+)\)", RegexOptions.Compiled);

    public static IReadOnlyList<string> ParseImageUrls(string content)
    {
        if (string.IsNullOrEmpty(content)) return [];

        var result = new HashSet<string>();
        foreach (Match match in ImagePattern.Matches(content))
            result.Add(match.Groups[1].Value);

        return result.ToList();
    }

    public static string GetSurroundingContext(string content, string imageUrl, int linesBefore = 3, int linesAfter = 3)
    {
        var lines = content.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Contains(imageUrl)) continue;

            var start = Math.Max(0, i - linesBefore);
            var end = Math.Min(lines.Length - 1, i + linesAfter);
            var contextLines = lines[start..(end + 1)].ToArray();
            contextLines[i - start] = "[image]";
            return string.Join('\n', contextLines);
        }
        return string.Empty;
    }

    public static string ReplaceImageWithDescription(string content, string imageUrl, string description)
    {
        return ImagePattern.Replace(content, match =>
            match.Groups[1].Value == imageUrl
                ? $"[圖片描述: {description}]"
                : match.Value);
    }
}
