using Domain.AI;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DN = Domain.Notes;

namespace Infrastructure.Search;

internal sealed class NoteSearcher(AppDbContext db, IEmbedder embedder) : INoteSearcher
{
    public async Task<IReadOnlyList<NoteSearchResult>> SearchAsync(UserId userId, string query, CancellationToken ct = default)
    {
        var queryVector = await embedder.EmbedAsync(query, ct);

        var rows = await db.Set<DN.NoteStructure>()
            .Where(s => db.Notes.Any(n => n.UserId == userId && n.Id == s.NoteId))
            .Include(s => s.Chunks).ThenInclude(c => c.Embedding)
            .Join(db.Notes.Where(n => n.UserId == userId),
                s => s.NoteId.Value,
                n => n.Id.Value,
                (s, n) => new { s, NoteId = n.Id, n.Title })
            .ToListAsync(ct);

        return rows
            .SelectMany(r => r.s.Chunks
                .Where(c => c.Embedding != null)
                .Select(c => new
                {
                    r.NoteId,
                    r.Title,
                    Score = CosineSimilarity(queryVector, c.Embedding!.Vector)
                }))
            .GroupBy(x => x.NoteId)
            .Select(g => new NoteSearchResult(new DN.NoteId(g.Key.Value), g.First().Title, g.Max(x => x.Score)))
            .OrderByDescending(r => r.Score)
            .Take(10)
            .ToList();
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        var dot = 0f;
        var magA = 0f;
        var magB = 0f;
        for (var i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        return magA == 0 || magB == 0 ? 0f : dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
    }
}
