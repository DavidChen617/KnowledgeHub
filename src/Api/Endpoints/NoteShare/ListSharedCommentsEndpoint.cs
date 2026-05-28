using Application.Comments;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;

namespace Api.Endpoints.NoteShare;

public sealed class ListSharedCommentsEndpoint : IGroupedEndpoint<ShareGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{token}/comments", HandleAsync)
            .Produces<GetCommentsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        string token,
        IDispatcher dispatcher,
        INoteRepository noteRepository,
        CancellationToken ct)
    {
        var note = await noteRepository.GetBySharedTokenAsync(token, ct);
        if (note is null) return Results.NotFound();

        var result = await dispatcher.Send(
            new GetCommentsQuery(note.Id, null, token), ct);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}
