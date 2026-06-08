using Api.Extensions;
using Application.Auth;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.OAuth;

public sealed class RefreshTokenEndpoint : IGroupedEndpoint<OAuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/refresh", HandleAsync)
            .Produces<Response<TokenResponse>>()
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> HandleAsync(
        RefreshRequest req,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new RenewTokenCommandRequest(req.RefreshToken), ct);
        return result.ToHttpResult();
    }
}

public record RefreshRequest(string RefreshToken);
