using CoreMesh.Dispatching.Abstractions;
using Domain.AI;
using Domain.Users;

namespace Application.Notes;

public record SearchQueryRequest(UserId UserId, string Query) : IRequest<SearchQueryResponse>;

public record SearchQueryResponse(IReadOnlyList<NoteSearchResult> Results);

public class SearchHandler(INoteSearcher noteSearcher)
    : IRequestHandler<SearchQueryRequest, SearchQueryResponse>
{
    public async Task<SearchQueryResponse> Handle(SearchQueryRequest query, CancellationToken cancellationToken = default)
    {
        var results = await noteSearcher.SearchAsync(query.UserId, query.Query, cancellationToken);

        return new SearchQueryResponse(results);
    }
}
