using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;
using Domain.Users;
using ShareKernal;

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
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var result = await dispatcher.Send(new DeleteCategoryCommandRequest(new CategoryId(id), currentUser.Id), ct);

        if (!result.IsSuccess)
            return result.Error.Type switch
            {
                ErrorType.NotFound => Results.NotFound(),
                _ => Results.Conflict(new { error = result.Error.Description })
            };

        return Results.NoContent();
    }
}
