using Domain.Notes;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class NoteRepository(AppDbContext db) : INoteRepository
{
    public async Task AddAsync(Note note, CancellationToken ct = default)
    {
        await db.Notes.AddAsync(note, ct);
    }

    public async Task<Note?> GetByIdAsync(NoteId id, CancellationToken ct = default)
    {
        return await db.Notes
            .Include(n => n.Structures).ThenInclude(s => s.Chunks).ThenInclude(c => c.Embedding)
            .Include(n => n.Images)
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<Note?> GetByIdAndUserIdAsync(NoteId id, UserId userId, CancellationToken ct = default)
    {
        return await db.Notes
            .Include(n => n.Structures).ThenInclude(s => s.Chunks).ThenInclude(c => c.Embedding)
            .Include(n => n.Images)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);
    }

    public async Task<IReadOnlyList<Note>> GetAllByUserIdAsync(UserId userId, CancellationToken ct = default)
    {
        return await db.Notes
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Note>> SearchByTitleAsync(UserId userId, string title, CancellationToken ct = default)
    {
        return await db.Notes
            .Where(n => n.UserId == userId && EF.Functions.ILike(n.Title, $"%{title}%"))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(ct);
    }

    public Task UpdateAsync(Note note, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Note note, CancellationToken ct = default)
    {
        db.Notes.Remove(note);
        return Task.CompletedTask;
    }
}
