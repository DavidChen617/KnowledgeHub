using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.NoteShare;

public sealed class GetSharedNoteEndpoint : IGroupedEndpoint<ShareGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{token}", HandleAsync)
            .Produces<GetNoteByTokenQueryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        string token,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var note = await dispatcher.Send(new GetNoteByTokenQueryRequest(token), ct);
        return note is null ? Results.NotFound() : Results.Ok(note);
    }
}
