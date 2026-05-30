using Domain.Shared;

namespace Domain.AI;

public enum ChunkSource { Raw, Structured }

public class Chunk<T> : Entity<Guid>
{
    public int Index { get; }
    public T Value { get; }
    public ChunkSource Source { get; }
    public Embedding? Embedding { get; private set; }

    private Chunk(Guid id, int index, T value, ChunkSource source) : base(id)
    {
        Index = index;
        Value = value;
        Source = source;
    }

    public static Chunk<T> Create(int index, T value, ChunkSource source = ChunkSource.Structured) =>
        new(Guid.NewGuid(), index, value, source);

    public void SetEmbedding(float[] vector) =>
        Embedding = Embedding.Create(Id, vector);
}
