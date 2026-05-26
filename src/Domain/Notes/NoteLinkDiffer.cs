namespace Domain.Notes;

public record NoteLinkDiffResult(IReadOnlyList<NoteId> ToAdd, IReadOnlyList<NoteId> ToRemove);

public static class NoteLinkDiffer
{
    public static NoteLinkDiffResult Diff(IEnumerable<NoteId> current, IEnumerable<NoteId> next)
    {
        var currentSet = current.ToHashSet();
        var nextSet = next.ToHashSet();

        var toAdd = nextSet.Except(currentSet).ToList();
        var toRemove = currentSet.Except(nextSet).ToList();

        return new NoteLinkDiffResult(toAdd, toRemove);
    }
}
