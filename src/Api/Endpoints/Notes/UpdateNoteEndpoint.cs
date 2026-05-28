using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;
using Domain.Notes;
using Domain.Users;

namespace Api.Endpoints.Notes;

public sealed class UpdateNoteEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", HandleAsync)
            .Produces<UpdateNoteCommandResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateNoteRequest req,
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var categoryId = req.CategoryId.HasValue ? new CategoryId(req.CategoryId.Value) : null;

        var result = await dispatcher.Send(
            new UpdateNoteCommandRequest(new NoteId(id), currentUser.Id, req.Title, req.Content, categoryId), ct);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}

public record UpdateNoteRequest(string? Title = null, string? Content = null, Guid? CategoryId = null);
