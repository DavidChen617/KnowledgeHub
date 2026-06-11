using Api.Extensions;
using Application.Comments;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Comments;
using Domain.Notes;
using Domain.Users;

namespace Api.Endpoints.Notes;

public sealed class AddCommentEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/comments", HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        AddCommentRequest req,
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var parentId = req.ParentCommentId.HasValue ? new CommentId(req.ParentCommentId.Value) : null;
        var result = await dispatcher.Send(
            new AddCommentCommand(new NoteId(id), currentUser.Id, req.Content, parentId, null), ct);

        return result.ToNoContent();
    }
}

public record AddCommentRequest(string Content, Guid? ParentCommentId = null);
