using CoreMesh.Dispatching.Abstractions;
using Domain.AI;
using Domain.Users;
using ShareKernal;

namespace Application.Notes;

public record SearchQueryRequest(UserId UserId, string Query) : IRequest<Result<SearchQueryResponse>>;

public record SearchQueryResponse(IReadOnlyList<NoteSearchResultDto> Results);

public record NoteSearchResultDto(Guid NoteId, string Title, float Score);

public class SearchHandler(INoteSearcher noteSearcher)
    : IRequestHandler<SearchQueryRequest, Result<SearchQueryResponse>>
{
    public async Task<Result<SearchQueryResponse>> Handle(SearchQueryRequest query, CancellationToken cancellationToken = default)
    {
        var searchResult = await noteSearcher.SearchAsync(query.UserId, query.Query, cancellationToken);
        if (!searchResult.IsSuccess) return searchResult.Error;

        return Result.Success(new SearchQueryResponse(
            searchResult.Value.Select(r => new NoteSearchResultDto(r.NoteId.Value, r.Title, r.Score)).ToList()));
    }
}
