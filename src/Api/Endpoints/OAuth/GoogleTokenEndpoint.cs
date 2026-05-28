using Application.Auth;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.OAuth;

public sealed class GoogleTokenEndpoint : IGroupedEndpoint<OAuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/google/token", HandleAsync)
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        GoogleTokenRequest req,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new ExchangeTokenCommandRequest(req.IdToken), ct);
        return result is null ? Results.Unauthorized() : Results.Ok(result);
    }
}

public record GoogleTokenRequest(string IdToken);
