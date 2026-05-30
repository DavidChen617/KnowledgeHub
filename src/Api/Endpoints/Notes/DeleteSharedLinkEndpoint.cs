using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;
using Domain.Users;

namespace Api.Endpoints.Notes;

public sealed class DeleteSharedLinkEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}/share", HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new DeleteSharedLinkCommandRequest(new NoteId(id), currentUser.Id), ct);
        return result.ToNoContent();
    }
}
