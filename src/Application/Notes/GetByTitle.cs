using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Users;

namespace Application.Notes;

public record GetByTitleQueryRequest(UserId UserId, string Title) : IRequest<GetByTitleQueryResponse>;

public record GetByTitleQueryResponse(IReadOnlyList<NoteSummary> Notes);

public class GetByTitleHandler(INoteRepository noteRepository)
    : IRequestHandler<GetByTitleQueryRequest, GetByTitleQueryResponse>
{
    public async Task<GetByTitleQueryResponse> Handle(GetByTitleQueryRequest query, CancellationToken cancellationToken = default)
    {
        var notes = await noteRepository.SearchByTitleAsync(query.UserId, query.Title, cancellationToken);

        var summaries = notes
            .Select(n => new NoteSummary(n.Id.Value, n.Title, n.UpdatedAt))
            .ToList();

        return new GetByTitleQueryResponse(summaries);
    }
}
