using Domain.Notes;

namespace Domain.Comments;

public interface ICommentRepository
{
    Task AddAsync(Comment comment, CancellationToken ct = default);
    Task<Comment?> GetByIdAsync(CommentId id, CancellationToken ct = default);
    Task<IReadOnlyList<Comment>> GetByNoteIdAsync(NoteId noteId, CancellationToken ct = default);
    Task DeleteAsync(Comment comment, CancellationToken ct = default);
}
