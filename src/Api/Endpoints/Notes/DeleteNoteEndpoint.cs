using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;

namespace Api.Endpoints.Notes;

public sealed class DeleteNoteEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IDispatcher dispatcher,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            if (!ctx.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var result = await dispatcher.Send(
                new DeleteNoteCommandRequest(new NoteId(id), userId), ct);

            return result is null ? Results.NotFound() : Results.NoContent();
        });
    }
}
