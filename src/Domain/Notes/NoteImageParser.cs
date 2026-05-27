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
}
