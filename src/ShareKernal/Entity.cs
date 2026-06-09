namespace ShareKernal;

public abstract class Entity<TId>
{
    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;   
    }

    public TId Id { get; }

    public DateTime CreatedAt { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        return Id!.Equals(other.Id);
    }

    public override int GetHashCode() => Id!.GetHashCode();
}
