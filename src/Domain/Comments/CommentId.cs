using Domain.Shared;

namespace Domain.Comments;

public sealed class CommentId : ValueObject
{
    public Guid Value { get; }

    public CommentId(Guid value) => Value = value;

    public static CommentId New() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
