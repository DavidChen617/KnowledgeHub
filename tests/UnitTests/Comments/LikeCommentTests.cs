using Application.Comments;
using Domain.Comments;
using Domain.Users;
using ShareKernal;
using NoteId = Domain.Notes.NoteId;

namespace UnitTests.Comments;

public class LikeCommentTests
{
    [Fact]
    public async Task Given_CommentExists_When_UserLikes_Then_Succeeds()
    {
        var comment = Comment.Create(NoteId.New(), UserId.New(), "內容").Value;
        var repo = new FakeCommentRepository(comment, existingLike: null);
        var handler = new LikeCommentHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new LikeCommentCommandRequest(comment.Id, UserId.New()));

        Assert.True(result.IsSuccess);
        Assert.True(repo.LikeWasAdded);
    }

    [Fact]
    public async Task Given_CommentAlreadyLiked_When_UserLikes_Then_ReturnsAlreadyLikedError()
    {
        var userId = UserId.New();
        var comment = Comment.Create(NoteId.New(), userId, "內容").Value;
        var existingLike = CommentLike.Create(comment.Id, userId);
        var repo = new FakeCommentRepository(comment, existingLike);
        var handler = new LikeCommentHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new LikeCommentCommandRequest(comment.Id, userId));

        Assert.False(result.IsSuccess);
        Assert.Equal(CommentErrors.AlreadyLiked.Code, result.Error.Code);
        Assert.False(repo.LikeWasAdded);
    }

    [Fact]
    public async Task Given_CommentNotFound_When_UserLikes_Then_ReturnsNotFound()
    {
        var repo = new FakeCommentRepository(returnComment: null, existingLike: null);
        var handler = new LikeCommentHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new LikeCommentCommandRequest(CommentId.New(), UserId.New()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
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

file class FakeCommentRepository(Comment? returnComment, CommentLike? existingLike) : ICommentRepository
{
    public bool LikeWasAdded { get; private set; }

    public Task<Comment?> GetByIdAsync(CommentId id, CancellationToken ct = default) =>
        Task.FromResult(returnComment);

    public Task<CommentLike?> FindLikeAsync(CommentId commentId, UserId userId, CancellationToken ct = default) =>
        Task.FromResult(existingLike);

    public Task AddLikeAsync(CommentLike like, CancellationToken ct = default)
    {
        LikeWasAdded = true;
        return Task.CompletedTask;
    }

    public Task AddAsync(Comment comment, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<Comment>> GetByNoteIdAsync(NoteId noteId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Comment>>([]);
    public Task DeleteAsync(Comment comment, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteLikeAsync(CommentLike like, CancellationToken ct = default) => Task.CompletedTask;
    public Task<Dictionary<CommentId, int>> GetLikeCountsAsync(IEnumerable<CommentId> commentIds, CancellationToken ct = default) =>
        Task.FromResult(new Dictionary<CommentId, int>());
    public Task<HashSet<CommentId>> GetLikedByUserAsync(IEnumerable<CommentId> commentIds, UserId userId, CancellationToken ct = default) =>
        Task.FromResult(new HashSet<CommentId>());
}
