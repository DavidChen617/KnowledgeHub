using System.Text.RegularExpressions;

namespace Domain.Notes;

public sealed class NoteContent
{
    private static readonly Regex LinkPattern  = new(@"\[\[([^\[\]]+)\]\]",      RegexOptions.Compiled);
    private static readonly Regex ImagePattern = new(@"!\[[^\]]*\]\(([^)]+)\)", RegexOptions.Compiled);

    public string Value { get; }
    public IReadOnlyList<NoteId> LinkedNoteIds { get; }
    public IReadOnlyList<string> ImageUrls { get; }

    public NoteContent(string value)
    {
        Value          = value ?? string.Empty;
        LinkedNoteIds  = ParseLinks(Value);
        ImageUrls      = ParseImages(Value);
    }

    public string GetSurroundingContext(string imageUrl, int linesBefore = 3, int linesAfter = 3)
    {
        var lines = Value.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Contains(imageUrl)) continue;

            var start        = Math.Max(0, i - linesBefore);
            var end          = Math.Min(lines.Length - 1, i + linesAfter);
            var contextLines = lines[start..(end + 1)].ToArray();
            contextLines[i - start] = "[image]";
            return string.Join('\n', contextLines);
        }
        return string.Empty;
    }

    public static string ReplaceImageWithDescription(string content, string imageUrl, string description) =>
        ImagePattern.Replace(content, match =>
            match.Groups[1].Value == imageUrl
                ? $"[圖片描述: {description}]"
                : match.Value);

    private static IReadOnlyList<NoteId> ParseLinks(string value)
    {
        if (string.IsNullOrEmpty(value)) return [];
        var result = new HashSet<Guid>();
        foreach (Match match in LinkPattern.Matches(value))
            if (Guid.TryParse(match.Groups[1].Value, out var guid))
                result.Add(guid);
        return result.Select(g => new NoteId(g)).ToList();
    }

    private static IReadOnlyList<string> ParseImages(string value)
    {
        if (string.IsNullOrEmpty(value)) return [];
        var result = new HashSet<string>();
        foreach (Match match in ImagePattern.Matches(value))
            result.Add(match.Groups[1].Value);
        return result.ToList();
    }

    public static implicit operator string(NoteContent content) => content.Value;
    public static implicit operator NoteContent(string value) => new(value);

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is NoteContent other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
