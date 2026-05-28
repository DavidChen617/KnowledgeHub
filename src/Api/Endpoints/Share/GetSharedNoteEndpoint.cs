using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.Share;

public sealed class GetSharedNoteEndpoint : IGroupedEndpoint<ShareGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{token}", HandleAsync)
            .Produces<SharedNoteResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        string token,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var note = await dispatcher.Send(new GetNoteByTokenQuery(token), ct);
        return note is null ? Results.NotFound() : Results.Ok(note);
    }
}
