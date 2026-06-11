using Api.Extensions;
using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Users;

namespace Api.Endpoints.Categories;

public sealed class ListCategoriesEndpoint : IGroupedEndpoint<CategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
            .Produces<Response<ListCategoriesDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new ListCategoriesQuery(currentUser.Id), ct);
        return result.ToHttpResult();
    }
}
