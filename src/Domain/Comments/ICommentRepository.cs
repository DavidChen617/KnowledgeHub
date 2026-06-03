using Domain.Notes;
using Domain.Users;

namespace Domain.Comments;

public interface ICommentRepository
{
    Task AddAsync(Comment comment, CancellationToken ct = default);
    Task<Comment?> GetByIdAsync(CommentId id, CancellationToken ct = default);
    Task<IReadOnlyList<Comment>> GetByNoteIdAsync(NoteId noteId, CancellationToken ct = default);
    Task DeleteAsync(Comment comment, CancellationToken ct = default);
    Task AddLikeAsync(CommentLike like, CancellationToken ct = default);
    Task<CommentLike?> FindLikeAsync(CommentId commentId, UserId userId, CancellationToken ct = default);
    Task DeleteLikeAsync(CommentLike like, CancellationToken ct = default);
    Task<Dictionary<CommentId, int>> GetLikeCountsAsync(IEnumerable<CommentId> commentIds, CancellationToken ct = default);
    Task<HashSet<CommentId>> GetLikedByUserAsync(IEnumerable<CommentId> commentIds, UserId userId, CancellationToken ct = default);
}
