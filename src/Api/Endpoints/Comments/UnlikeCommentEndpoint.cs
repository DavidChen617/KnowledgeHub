using Application.Comments;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Comments;
using Domain.Users;

namespace Api.Endpoints.Comments;

public sealed class UnlikeCommentEndpoint : IGroupedEndpoint<CommentsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}/like", HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var result = await dispatcher.Send(new UnlikeCommentCommandRequest(new CommentId(id), currentUser.Id), ct);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }
}
