using Api.Extensions;
using Application.Auth;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.OAuth;

public sealed class GoogleTokenEndpoint : IGroupedEndpoint<OAuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/google/token", HandleAsync)
            .Produces<Response<TokenResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> HandleAsync(
        GoogleTokenRequest req,
        HttpContext ctx,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
        var result = await dispatcher.Send(new ExchangeTokenCommand(req.IdToken, baseUrl), ct);
        return result.ToHttpResult();
    }
}

public record GoogleTokenRequest(string IdToken);
