using Application.Comments;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;
using Domain.Users;

namespace Api.Endpoints.Notes;

public sealed class ListCommentsEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}/comments", HandleAsync)
            .Produces<GetCommentsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var result = await dispatcher.Send(
            new GetCommentsQuery(new NoteId(id), currentUser.Id, null), ct);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}
