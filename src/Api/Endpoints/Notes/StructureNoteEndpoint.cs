using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;
using Domain.Users;
using ShareKernal;

namespace Api.Endpoints.Notes;

public sealed class StructureNoteEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/structure", HandleAsync)
            .Produces<StructureNoteCommandResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        StructureNoteRequest req,
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var result = await dispatcher.Send(
            new StructureNoteCommandRequest(new NoteId(id), req.Prompt), ct);

        if (!result.IsSuccess)
            return result.Error.Type switch
            {
                ErrorType.NotFound => Results.NotFound(),
                ErrorType.ServiceUnavailable => Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, detail: result.Error.Description),
                _ => Results.Problem(detail: result.Error.Description)
            };

        return Results.Ok(result.Value);
    }
}

public record StructureNoteRequest(string Prompt);
