using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.NoteShare;

public sealed class UpdateSharedNoteEndpoint : IGroupedEndpoint<ShareGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPatch("/{token}", HandleAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        string token,
        UpdateSharedNoteRequest req,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new UpdateNoteByTokenCommandRequest(token, req.Content), ct);
        return result.ToNoContent();
    }
}

public record UpdateSharedNoteRequest(string Content);
