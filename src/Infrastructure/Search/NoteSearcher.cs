using Domain.NoteStructure;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using ShareKernal;
using DN = Domain.Notes;

namespace Infrastructure.Search;

internal sealed class NoteSearcher(AppDbContext db, IEmbedder embedder) : INoteSearcher
{
    public async Task<Result<IReadOnlyList<NoteSearchResult>>> SearchAsync(UserId userId, string query, CancellationToken ct = default)
    {
        var embedResult = await embedder.EmbedAsync(query, ct);
        if (!embedResult.IsSuccess) return embedResult.Error;

        var pgVector = new Vector(embedResult.Value);

        var results = await db.Database.SqlQuery<NoteSearchRow>($"""
            SELECT n.id AS "NoteId", n.title AS "Title",
                   MIN(e.vector <=> {pgVector}) AS "Distance"
            FROM note_structure_chunk_embeddings e
            JOIN note_structure_chunks c ON c.id = e.chunk_id
            JOIN note_structures s       ON s.id = c.note_structure_id
            JOIN notes n                 ON n.id = s.note_id
            WHERE n.user_id = {userId.Value}
            GROUP BY n.id, n.title
            ORDER BY "Distance"
            LIMIT 10
            """)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<NoteSearchResult>>(
            results.Select(r => new NoteSearchResult(new DN.NoteId(r.NoteId), r.Title, 1f - r.Distance)).ToList());
    }

    private sealed record NoteSearchRow(Guid NoteId, string Title, float Distance);
}
