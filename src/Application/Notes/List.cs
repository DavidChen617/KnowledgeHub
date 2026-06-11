using Application.Interfaces;
using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Users;
using ShareKernal;

namespace Application.Notes;

public record ListNotesQuery(UserId UserId) : IRequest<Result<ListNotesDto>>;

public record ListNotesDto(IReadOnlyList<NoteSummary> Notes);

public record NoteSummary(Guid NoteId, string Title, DateTime UpdatedAt, Guid? CategoryId);

public class ListHandler(INoteRepository noteRepository, ICacher cacher)
    : IRequestHandler<ListNotesQuery, Result<ListNotesDto>>
{
    public async Task<Result<ListNotesDto>> Handle(ListNotesQuery query, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.NoteList(query.UserId.Value);

        var cached = await cacher.GetAsync<ListNotesDto>(key, cancellationToken);
        if (cached is not null) return Result.Success(cached);

        var notes = await noteRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);
        var response = new ListNotesDto(notes
            .Select(n => new NoteSummary(n.Id.Value, n.Title, n.UpdatedAt, n.CategoryId?.Value))
            .ToList());

        await cacher.SetAsync(key, response, TimeSpan.FromMinutes(5), cancellationToken);

        return Result.Success(response);
    }
}
