using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;

namespace Api.Endpoints.Notes;

public sealed class StructureNoteEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/structure", async (
            Guid id,
            StructureNoteRequest req,
            IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var result = await dispatcher.Send(
                new StructureNoteCommandRequest(new NoteId(id), req.Prompt), ct);

            return result is null ? Results.NotFound() : Results.Ok(result);
        });
    }
}

public record StructureNoteRequest(string Prompt);
