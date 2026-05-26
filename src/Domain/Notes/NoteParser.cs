using System.Text.RegularExpressions;

namespace Domain.Notes;

public static class NoteParser
{
    private static readonly Regex LinkPattern = new(@"\[\[([^\[\]]+)\]\]", RegexOptions.Compiled);

    public static IReadOnlyList<Guid> ParseNoteLinks(string content)
    {
        if (string.IsNullOrEmpty(content)) return [];

        var result = new HashSet<Guid>();
        foreach (Match match in LinkPattern.Matches(content))
        {
            if (Guid.TryParse(match.Groups[1].Value, out var guid))
                result.Add(guid);
        }

        return result.ToList();
    }
}
