using Domain.Shared;

namespace Domain.AI;

public class Chunk<T> : Entity<Guid>
{
    public int Index { get; }
    public T Artifact { get; }
    public Embedding? Embedding { get; private set; }

    private Chunk(Guid id, int index, T artifact) : base(id)
    {
        Index = index;
        Artifact = artifact;
    }

    public static Chunk<T> Create(int index, T artifact) =>
        new(Guid.NewGuid(), index, artifact);

    public void SetEmbedding(float[] vector) =>
        Embedding = Embedding.Create(Id, vector);
}
