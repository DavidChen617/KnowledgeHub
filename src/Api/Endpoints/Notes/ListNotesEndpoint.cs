using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Users;

namespace Api.Endpoints.Notes;

public sealed class ListNotesEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
            .Produces<Response<ListNotesDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new ListNotesQuery(currentUser.Id), ct);
        return result.ToHttpResult();
    }
}
