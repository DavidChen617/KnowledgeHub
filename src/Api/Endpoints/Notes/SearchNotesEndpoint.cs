using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.Notes;

public sealed class SearchNotesEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/search", HandleAsync)
            .Produces<SearchQueryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        string q,
        IDispatcher dispatcher,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!ctx.TryGetUserId(out var userId))
            return Results.Unauthorized();

        var result = await dispatcher.Send(new SearchQueryRequest(userId, q), ct);

        return Results.Ok(result);
    }
}
