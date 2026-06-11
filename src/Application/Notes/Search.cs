using CoreMesh.Dispatching.Abstractions;
using Domain.NoteStructure;
using Domain.Users;
using ShareKernal;

namespace Application.Notes;

public record SearchQuery(UserId UserId, string Query) : IRequest<Result<SearchDto>>;

public record SearchDto(IReadOnlyList<NoteSearchResultDto> Results);

public record NoteSearchResultDto(Guid NoteId, string Title, float Score);

public class SearchHandler(INoteSearcher noteSearcher)
    : IRequestHandler<SearchQuery, Result<SearchDto>>
{
    public async Task<Result<SearchDto>> Handle(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var searchResult = await noteSearcher.SearchAsync(query.UserId, query.Query, cancellationToken);
        if (!searchResult.IsSuccess) return searchResult.Error;

        return Result.Success(new SearchDto(
            searchResult.Value.Select(r => new NoteSearchResultDto(r.NoteId.Value, r.Title, r.Score)).ToList()));
    }
}
