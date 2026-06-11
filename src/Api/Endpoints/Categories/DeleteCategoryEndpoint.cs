using Api.Extensions;
using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;
using Domain.Users;

namespace Api.Endpoints.Categories;

public sealed class DeleteCategoryEndpoint : IGroupedEndpoint<CategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new DeleteCategoryCommand(new CategoryId(id), currentUser.Id), ct);

        return result.ToNoContent();
    }
}
