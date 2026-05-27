using Api.Extensions;
using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;

namespace Api.Endpoints.Categories;

public sealed class ListCategoriesEndpoint : IGroupedEndpoint<CategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
            .Produces<ListCategoriesQueryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        IDispatcher dispatcher,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!ctx.TryGetUserId(out var userId))
            return Results.Unauthorized();

        var result = await dispatcher.Send(new ListCategoriesQueryRequest(userId), ct);

        return Results.Ok(result);
    }
}
