using Application.Interfaces;
using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Users;
using ShareKernal;

namespace Application.Notes;

public record ListQueryRequest(UserId UserId) : IRequest<Result<ListQueryResponse>>;

public record ListQueryResponse(IReadOnlyList<NoteSummary> Notes);

public record NoteSummary(Guid NoteId, string Title, DateTime UpdatedAt);

public class ListHandler(INoteRepository noteRepository, ICacher cacher)
    : IRequestHandler<ListQueryRequest, Result<ListQueryResponse>>
{
    public async Task<Result<ListQueryResponse>> Handle(ListQueryRequest query, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.NoteList(query.UserId.Value);

        var cached = await cacher.GetAsync<ListQueryResponse>(key, cancellationToken);
        if (cached is not null) return Result.Success(cached);

        var notes = await noteRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);
        var response = new ListQueryResponse(notes
            .Select(n => new NoteSummary(n.Id.Value, n.Title, n.UpdatedAt))
            .ToList());

        await cacher.SetAsync(key, response, TimeSpan.FromMinutes(5), cancellationToken);

        return Result.Success(response);
    }
}
