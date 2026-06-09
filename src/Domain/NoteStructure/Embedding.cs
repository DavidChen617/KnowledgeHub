using ShareKernal;


namespace Domain.NoteStructure;

public class Embedding : Entity<Guid>
{
    public Guid ChunkId { get; }
    public float[] Vector { get; }

    private Embedding(Guid id, Guid chunkId, float[] vector) : base(id)
    {
        ChunkId = chunkId;
        Vector = vector;
    }

    public static Embedding Create(Guid chunkId, float[] vector) =>
        new(Guid.NewGuid(), chunkId, vector);
}
