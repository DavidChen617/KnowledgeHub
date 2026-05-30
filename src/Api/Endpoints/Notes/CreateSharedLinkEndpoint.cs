using Api.Extensions;
using Application.Notes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;
using Domain.Users;

namespace Api.Endpoints.Notes;

public sealed class CreateSharedLinkEndpoint : IGroupedEndpoint<NotesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/share", HandleAsync)
            .Produces<CreateSharedLinkResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        CreateShareRequest req,
        User? currentUser,
        HttpContext ctx,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var permission = req.Permission == "readwrite" ? SharePermission.ReadWrite : SharePermission.Read;
        var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";

        var result = await dispatcher.Send(new CreateSharedLinkCommandRequest(new NoteId(id), currentUser.Id, permission), ct);

        return result.ToHttpResult(v => Results.Ok(new CreateSharedLinkResponse($"{baseUrl}/share/{v.Token}", req.Permission)));
    }
}

public record CreateShareRequest(string Permission = "read");
public record CreateSharedLinkResponse(string Url, string Permission);
