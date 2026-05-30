using Domain.AI;
using Domain.Shared;

namespace Domain.Notes;

public class NoteStructure : Entity<Guid>
{
    private readonly List<Chunk<string>> _chunks = [];

    public NoteId NoteId { get; }
    public string Prompt { get; }
    public string Content { get; }
    public string Description { get; }

    public IReadOnlyList<Chunk<string>> Chunks => _chunks;

    private NoteStructure(Guid id, NoteId noteId, string prompt, string content, string description) : base(id)
    {
        NoteId = noteId;
        Prompt = prompt;
        Content = content;
        Description = description;
    }

    public static NoteStructure Create(NoteId noteId, string prompt, string content, string description,
        IReadOnlyList<(int Index, string Text)> chunks)
    {
        var structure = new NoteStructure(Guid.NewGuid(), noteId, prompt, content, description);

        foreach (var (index, text) in chunks)
            structure._chunks.Add(Chunk<string>.Create(index, text));

        return structure;
    }

    public List<string> GetChunks() =>
        Chunks.Select(c => c.Value).ToList();

    public void SetEmbedding(float[][] vector)
    {
        for (var i = 0; i < Chunks.Count; ++i)
            Chunks[i].SetEmbedding(vector[i]);
    }
}
