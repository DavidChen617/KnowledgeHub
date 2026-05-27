using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;

namespace Api.Endpoints.Notes;

public sealed class StructureNoteEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/structure", HandleAsync)
            .Produces<StructureNoteCommandResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        StructureNoteRequest req,
        IDispatcher dispatcher,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!ctx.TryGetUserId(out var userId))
            return Results.Unauthorized();

        var result = await dispatcher.Send(
            new StructureNoteCommandRequest(new NoteId(id), req.Prompt), ct);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}

public record StructureNoteRequest(string Prompt);
