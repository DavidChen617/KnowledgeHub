using Api.Extensions;
using Application.Comments;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Comments;
using Domain.Notes;
using Domain.Users;
namespace Api.Endpoints.NoteShare;

public sealed class AddSharedCommentEndpoint : IGroupedEndpoint<ShareGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{token}/comments", HandleAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        string token,
        AddSharedCommentRequest req,
        User currentUser,
        IDispatcher dispatcher,
        INoteRepository noteRepository,
        CancellationToken ct)
    {
        var note = await noteRepository.GetBySharedTokenAsync(token, ct);
        if (note is null) return ResultExtensions.NotFound();

        var parentId = req.ParentCommentId.HasValue ? new CommentId(req.ParentCommentId.Value) : null;
        var result = await dispatcher.Send(
            new AddCommentCommandRequest(note.Id, currentUser.Id, req.Content, parentId, token), ct);

        return result.ToNoContent();
    }
}

public record AddSharedCommentRequest(string Content, Guid? ParentCommentId = null);
