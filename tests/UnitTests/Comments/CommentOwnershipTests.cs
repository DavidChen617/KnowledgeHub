using Application.Comments;
using Domain.Comments;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace UnitTests.Comments;

public class CommentOwnershipTests
{
    // ── EditComment ──────────────────────────────────────────────────────────

    [Fact]
    public async Task EditComment_Author_Succeeds()
    {
        var authorId = UserId.New();
        var comment = Comment.Create(NoteId.New(), authorId, "原始內容").Value;
        var repo = new FakeOwnershipCommentRepository(comment);
        var handler = new EditCommentHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new EditCommentCommandRequest(comment.Id, authorId, "更新內容"));

        Assert.True(result.IsSuccess);
        Assert.Equal("更新內容", comment.Content);
    }

    [Fact]
    public async Task EditComment_NonAuthor_ReturnsForbidden()
    {
        var authorId = UserId.New();
        var comment = Comment.Create(NoteId.New(), authorId, "原始內容").Value;
        var repo = new FakeOwnershipCommentRepository(comment);
        var handler = new EditCommentHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new EditCommentCommandRequest(comment.Id, UserId.New(), "嘗試修改"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal("原始內容", comment.Content);
    }

    [Fact]
    public async Task EditComment_NotFound_ReturnsNotFound()
    {
        var repo = new FakeOwnershipCommentRepository(returnComment: null);
        var handler = new EditCommentHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new EditCommentCommandRequest(CommentId.New(), UserId.New(), "內容"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    // ── DeleteComment ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteComment_Author_Succeeds()
    {
        var authorId = UserId.New();
        var comment = Comment.Create(NoteId.New(), authorId, "內容").Value;
        var repo = new FakeOwnershipCommentRepository(comment);
        var handler = new DeleteCommentHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new DeleteCommentCommandRequest(comment.Id, authorId));

        Assert.True(result.IsSuccess);
        Assert.True(repo.WasDeleted);
    }

    [Fact]
    public async Task DeleteComment_NonAuthor_ReturnsForbidden()
    {
        var authorId = UserId.New();
        var comment = Comment.Create(NoteId.New(), authorId, "內容").Value;
        var repo = new FakeOwnershipCommentRepository(comment);
        var handler = new DeleteCommentHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new DeleteCommentCommandRequest(comment.Id, UserId.New()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.False(repo.WasDeleted);
    }

    [Fact]
    public async Task DeleteComment_NotFound_ReturnsNotFound()
    {
        var repo = new FakeOwnershipCommentRepository(returnComment: null);
        var handler = new DeleteCommentHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new DeleteCommentCommandRequest(CommentId.New(), UserId.New()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.False(repo.WasDeleted);
    }
}

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public static readonly FakeUnitOfWork Instance = new();
    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file class FakeOwnershipCommentRepository(Comment? returnComment) : ICommentRepository
{
    public bool WasDeleted { get; private set; }

    public Task<Comment?> GetByIdAsync(CommentId id, CancellationToken ct = default) =>
        Task.FromResult(returnComment);

    public Task DeleteAsync(Comment comment, CancellationToken ct = default)
    {
        WasDeleted = true;
        return Task.CompletedTask;
    }

    public Task AddAsync(Comment comment, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<Comment>> GetByNoteIdAsync(NoteId noteId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Comment>>([]);
    public Task AddLikeAsync(CommentLike like, CancellationToken ct = default) => Task.CompletedTask;
    public Task<CommentLike?> FindLikeAsync(CommentId commentId, UserId userId, CancellationToken ct = default) =>
        Task.FromResult<CommentLike?>(null);
    public Task DeleteLikeAsync(CommentLike like, CancellationToken ct = default) => Task.CompletedTask;
    public Task<Dictionary<CommentId, int>> GetLikeCountsAsync(IEnumerable<CommentId> commentIds, CancellationToken ct = default) =>
        Task.FromResult(new Dictionary<CommentId, int>());
    public Task<HashSet<CommentId>> GetLikedByUserAsync(IEnumerable<CommentId> commentIds, UserId userId, CancellationToken ct = default) =>
        Task.FromResult(new HashSet<CommentId>());
}
