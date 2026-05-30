using Domain.Comments;
using Domain.Notes;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class CommentRepository(AppDbContext db) : ICommentRepository
{
    public async Task AddAsync(Comment comment, CancellationToken ct = default) =>
        await db.Comments.AddAsync(comment, ct);

    public Task<Comment?> GetByIdAsync(CommentId id, CancellationToken ct = default) =>
        db.Comments.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Comment>> GetByNoteIdAsync(NoteId noteId, CancellationToken ct = default) =>
        await db.Comments
            .Where(c => c.NoteId == noteId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public Task DeleteAsync(Comment comment, CancellationToken ct = default)
    {
        db.Comments.Remove(comment);
        return Task.CompletedTask;
    }

    public async Task AddLikeAsync(CommentLike like, CancellationToken ct = default) =>
        await db.CommentLikes.AddAsync(like, ct);

    public Task<CommentLike?> FindLikeAsync(CommentId commentId, UserId userId, CancellationToken ct = default) =>
        db.CommentLikes.FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId, ct);

    public Task DeleteLikeAsync(CommentLike like, CancellationToken ct = default)
    {
        db.CommentLikes.Remove(like);
        return Task.CompletedTask;
    }

    public async Task<Dictionary<CommentId, int>> GetLikeCountsAsync(IEnumerable<CommentId> commentIds, CancellationToken ct = default)
    {
        var ids = commentIds.Select(id => id.Value).ToList();
        var rows = await db.CommentLikes
            .Where(l => ids.Contains(EF.Property<Guid>(l, "comment_id")))
            .GroupBy(l => EF.Property<Guid>(l, "comment_id"))
            .Select(g => new { CommentId = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        return rows.ToDictionary(x => new CommentId(x.CommentId), x => x.Count);
    }
}
