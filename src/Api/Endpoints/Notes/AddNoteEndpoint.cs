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
            .Produces<Response<AddNoteCommandResponse>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        AddNoteRequest req,
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var categoryId = req.CategoryId.HasValue ? new CategoryId(req.CategoryId.Value) : null;

        var result = await dispatcher.Send(
            new AddNoteCommandRequest(currentUser.Id, req.Title, req.Content ?? string.Empty, categoryId), ct);

        return result.ToCreated(v => $"/api/notes/{v.NoteId}");
    }
}

public record AddNoteRequest(string Title, string? Content = null, Guid? CategoryId = null);
