using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;
using Domain.Users;

namespace Api.Endpoints.Notes;

public sealed class StructureNoteEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/structure", HandleAsync)
            .Produces<Response<StructureNoteCommandResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        StructureNoteRequest req,
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new StructureNoteCommandRequest(new NoteId(id), req.Prompt ?? "請將這篇筆記整理成有結構的格式，包含標題與重點摘要", currentUser.Id.Value), ct);

        return result.ToHttpResult();
    }
}

public record StructureNoteRequest(string? Prompt = null);
