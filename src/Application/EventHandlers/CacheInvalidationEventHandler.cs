using Application.Interfaces;
using CoreMesh.Outbox.Abstractions;
using Domain.Categories.Events;
using Domain.Comments.Events;
using Domain.Notes.Events;

namespace Application.EventHandlers;

public sealed class NoteCreatedCacheHandler(ICacher cacher) : IEventHandler<NoteCreatedEvent>
{
    public async Task HandleAsync(NoteCreatedEvent @event, CancellationToken ct = default) =>
        await Task.WhenAll(
            cacher.RemoveAsync(CacheKeys.NoteList(@event.UserId), ct),
            cacher.RemoveAsync(CacheKeys.NoteGraph(@event.UserId), ct));
}

public sealed class NoteUpdatedCacheHandler(ICacher cacher) : IEventHandler<NoteUpdatedEvent>
{
    public async Task HandleAsync(NoteUpdatedEvent @event, CancellationToken ct = default)
    {
        var tasks = new List<Task>
        {
            cacher.RemoveAsync(CacheKeys.NoteList(@event.UserId), ct),
            cacher.RemoveAsync(CacheKeys.NoteGraph(@event.UserId), ct)
        };
        if (@event.SharedLinkToken is not null)
            tasks.Add(cacher.RemoveAsync(CacheKeys.NoteByToken(@event.SharedLinkToken), ct));
        await Task.WhenAll(tasks);
    }
}

public sealed class NoteDeletedCacheHandler(ICacher cacher) : IEventHandler<NoteDeletedEvent>
{
    public async Task HandleAsync(NoteDeletedEvent @event, CancellationToken ct = default) =>
        await Task.WhenAll(
            cacher.RemoveAsync(CacheKeys.NoteList(@event.UserId), ct),
            cacher.RemoveAsync(CacheKeys.NoteGraph(@event.UserId), ct));
}

public sealed class SharedLinkCreatedCacheHandler(ICacher cacher) : IEventHandler<SharedLinkCreatedEvent>
{
    public async Task HandleAsync(SharedLinkCreatedEvent @event, CancellationToken ct = default)
    {
        if (@event.PreviousToken is not null)
            await cacher.RemoveAsync(CacheKeys.NoteByToken(@event.PreviousToken), ct);
    }
}

public sealed class SharedLinkDeletedCacheHandler(ICacher cacher) : IEventHandler<SharedLinkDeletedEvent>
{
    public async Task HandleAsync(SharedLinkDeletedEvent @event, CancellationToken ct = default) =>
        await cacher.RemoveAsync(CacheKeys.NoteByToken(@event.Token), ct);
}

public sealed class CategoryCreatedCacheHandler(ICacher cacher) : IEventHandler<CategoryCreatedEvent>
{
    public async Task HandleAsync(CategoryCreatedEvent @event, CancellationToken ct = default) =>
        await Task.WhenAll(
            cacher.RemoveAsync(CacheKeys.Categories(@event.UserId), ct),
            cacher.RemoveAsync(CacheKeys.NoteGraph(@event.UserId), ct));
}

public sealed class CategoryUpdatedCacheHandler(ICacher cacher) : IEventHandler<CategoryUpdatedEvent>
{
    public async Task HandleAsync(CategoryUpdatedEvent @event, CancellationToken ct = default) =>
        await Task.WhenAll(
            cacher.RemoveAsync(CacheKeys.Categories(@event.UserId), ct),
            cacher.RemoveAsync(CacheKeys.NoteGraph(@event.UserId), ct));
}

public sealed class CategoryDeletedCacheHandler(ICacher cacher) : IEventHandler<CategoryDeletedEvent>
{
    public async Task HandleAsync(CategoryDeletedEvent @event, CancellationToken ct = default) =>
        await Task.WhenAll(
            cacher.RemoveAsync(CacheKeys.Categories(@event.UserId), ct),
            cacher.RemoveAsync(CacheKeys.NoteGraph(@event.UserId), ct));
}

public sealed class CommentEditedCacheHandler(ICacher cacher) : IEventHandler<CommentEditedEvent>
{
    public async Task HandleAsync(CommentEditedEvent @event, CancellationToken ct = default) =>
        await cacher.RemoveAsync(CacheKeys.Comments(@event.NoteId), ct);
}

public sealed class CommentDeletedCacheHandler(ICacher cacher) : IEventHandler<CommentDeletedEvent>
{
    public async Task HandleAsync(CommentDeletedEvent @event, CancellationToken ct = default) =>
        await cacher.RemoveAsync(CacheKeys.Comments(@event.NoteId), ct);
}

public sealed class CommentLikedCacheHandler(ICacher cacher) : IEventHandler<CommentLikedEvent>
{
    public async Task HandleAsync(CommentLikedEvent @event, CancellationToken ct = default) =>
        await cacher.RemoveAsync(CacheKeys.Comments(@event.NoteId), ct);
}

public sealed class CommentUnlikedCacheHandler(ICacher cacher) : IEventHandler<CommentUnlikedEvent>
{
    public async Task HandleAsync(CommentUnlikedEvent @event, CancellationToken ct = default) =>
        await cacher.RemoveAsync(CacheKeys.Comments(@event.NoteId), ct);
}
