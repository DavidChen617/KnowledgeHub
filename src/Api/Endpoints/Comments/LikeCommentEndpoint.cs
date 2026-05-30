using Api.Extensions;
using Application.Comments;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Comments;
using Domain.Users;

namespace Api.Endpoints.Comments;

public sealed class LikeCommentEndpoint : IGroupedEndpoint<CommentsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/like", HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new LikeCommentCommandRequest(new CommentId(id), currentUser.Id), ct);

        return result.ToNoContent();
    }
}
