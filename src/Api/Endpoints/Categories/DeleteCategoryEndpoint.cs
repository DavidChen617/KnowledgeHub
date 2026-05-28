using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;
using Domain.Exceptions;
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
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        try
        {
            var result = await dispatcher.Send(new DeleteCategoryCommandRequest(new CategoryId(id), currentUser.Id), ct);
            return result is null ? Results.NotFound() : Results.NoContent();
        }
        catch (CategoryInUseException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }
}
