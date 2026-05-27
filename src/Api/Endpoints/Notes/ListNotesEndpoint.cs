using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.Notes;

public sealed class ListNotesEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
            .Produces<ListQueryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        IDispatcher dispatcher,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!ctx.TryGetUserId(out var userId))
            return Results.Unauthorized();

        var result = await dispatcher.Send(new ListQueryRequest(userId), ct);

        return Results.Ok(result);
    }
}
