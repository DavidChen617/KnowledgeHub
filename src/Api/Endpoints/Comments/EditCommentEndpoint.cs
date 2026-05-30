using Api.Extensions;
using Application.Comments;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Comments;
using Domain.Users;

namespace Api.Endpoints.Comments;

public sealed class EditCommentEndpoint : IGroupedEndpoint<CommentsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        EditCommentRequest req,
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var result = await dispatcher.Send(
            new EditCommentCommandRequest(new CommentId(id), currentUser.Id, req.Content), ct);

        return result.ToNoContent();
    }
}

public record EditCommentRequest(string Content);
