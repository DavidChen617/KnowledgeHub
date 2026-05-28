using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.Share;

public sealed class UpdateSharedNoteEndpoint : IGroupedEndpoint<ShareGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPatch("/{token}", HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        string token,
        UpdateSharedNoteRequest req,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new UpdateNoteByTokenCommand(token, req.Content), ct);

        return result switch
        {
            UpdateNoteByTokenResult.Success => Results.NoContent(),
            UpdateNoteByTokenResult.Forbidden => Results.Forbid(),
            _ => Results.NotFound()
        };
    }
}

public record UpdateSharedNoteRequest(string Content);
