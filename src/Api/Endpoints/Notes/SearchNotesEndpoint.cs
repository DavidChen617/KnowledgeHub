using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Users;
using ShareKernal;

namespace Api.Endpoints.Notes;

public sealed class SearchNotesEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/search", HandleAsync)
            .Produces<SearchQueryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> HandleAsync(
        string q,
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var result = await dispatcher.Send(new SearchQueryRequest(currentUser.Id, q), ct);

        if (!result.IsSuccess)
            return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, detail: result.Error.Description);

        return Results.Ok(result.Value);
    }
}
