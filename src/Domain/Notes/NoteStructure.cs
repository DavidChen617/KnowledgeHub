using Domain.AI;
using Domain.Shared;

namespace Domain.Notes;

public class NoteStructure : Entity<Guid>
{
    private readonly List<Chunk<string>> _chunks = [];

    public NoteId NoteId { get; }
    public string Prompt { get; }
    public string Content { get; }

    public IReadOnlyList<Chunk<string>> Chunks => _chunks;

    private NoteStructure(Guid id, NoteId noteId, string prompt, string content) : base(id)
    {
        NoteId = noteId;
        Prompt = prompt;
        Content = content;
    }

    public static NoteStructure Create(NoteId noteId, string prompt, string content, IReadOnlyList<(int Index, string Text)> chunks)
    {
        var structure = new NoteStructure(Guid.NewGuid(), noteId, prompt, content);

        foreach (var (index, text) in chunks)
            structure._chunks.Add(Chunk<string>.Create(index, text));

        return structure;
    }
}
