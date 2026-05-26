using Domain.Shared;

namespace Domain.Notes;

public sealed class NoteId : ValueObject
{
    public Guid Value { get; }

    public NoteId(Guid value) => Value = value;

    public static NoteId New() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
