using Api.Extensions;
using Application.Comments;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.NoteShare;

public sealed class ListSharedCommentsEndpoint : IGroupedEndpoint<ShareGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{token}/comments", HandleAsync)
            .Produces<GetCommentsQueryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        string token,
        IDispatcher dispatcher,
        INoteRepository noteRepository,
        CancellationToken ct)
    {
        var note = await noteRepository.GetBySharedTokenAsync(token, ct);
        if (note is null) return Results.Json(
            Response.Fail(new ProblemDetails { Status = StatusCodes.Status404NotFound }),
            statusCode: StatusCodes.Status404NotFound);

        var result = await dispatcher.Send(
            new GetCommentsQueryRequest(note.Id, null, token), ct);

        return result.ToHttpResult();
    }
}
