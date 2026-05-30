using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;
using Domain.Users;

namespace Api.Endpoints.Notes;

public sealed class AddNoteEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .Produces<AddNoteCommandResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        AddNoteRequest req,
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var categoryId = req.CategoryId.HasValue ? new CategoryId(req.CategoryId.Value) : null;

        var result = await dispatcher.Send(
            new AddNoteCommandRequest(currentUser.Id, req.Title, req.Content ?? string.Empty, categoryId), ct);

        return result.ToHttpResult(v => Results.Created($"/api/notes/{v.NoteId}", Response.Ok(v)));
    }
}

public record AddNoteRequest(string Title, string? Content = null, Guid? CategoryId = null);
