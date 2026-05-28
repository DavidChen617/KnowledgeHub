using Domain.Comments;
using Domain.Notes;
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
}
