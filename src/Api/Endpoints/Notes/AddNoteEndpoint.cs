using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;

namespace Api.Endpoints.Notes;

public sealed class AddNoteEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .Produces<AddNoteCommandResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(AddNoteRequest req,
        IDispatcher dispatcher,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!ctx.TryGetUserId(out var userId))
            return Results.Unauthorized();

        var categoryId = req.CategoryId.HasValue ? new CategoryId(req.CategoryId.Value) : null;

        var result = await dispatcher.Send(
            new AddNoteCommandRequest(userId, req.Title, req.Content ?? string.Empty, categoryId), ct);

        return Results.Created($"/api/notes/{result.NoteId.Value}", result);
    }
}

public record AddNoteRequest(string Title, string? Content = null, Guid? CategoryId = null);
