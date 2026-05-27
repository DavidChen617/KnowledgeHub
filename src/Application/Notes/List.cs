using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Users;

namespace Application.Notes;

public record ListQueryRequest(UserId UserId) : IRequest<ListQueryResponse>;

public record ListQueryResponse(IReadOnlyList<NoteSummary> Notes);

public record NoteSummary(NoteId NoteId, string Title, DateTime UpdatedAt);

public class ListHandler(INoteRepository noteRepository)
    : IRequestHandler<ListQueryRequest, ListQueryResponse>
{
    public async Task<ListQueryResponse> Handle(ListQueryRequest query, CancellationToken cancellationToken = default)
    {
        var notes = await noteRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);

        var summaries = notes
            .Select(n => new NoteSummary(n.Id, n.Title, n.UpdatedAt))
            .ToList();

        return new ListQueryResponse(summaries);
    }
}
