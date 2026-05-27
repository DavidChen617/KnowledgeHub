using Api.Extensions;
using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;
using Domain.Exceptions;

namespace Api.Endpoints.Categories;

public sealed class DeleteCategoryEndpoint : IGroupedEndpoint<CategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IDispatcher dispatcher,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            if (!ctx.TryGetUserId(out var userId))
                return Results.Unauthorized();

            try
            {
                var result = await dispatcher.Send(new DeleteCategoryCommandRequest(new CategoryId(id), userId), ct);
                return result is null ? Results.NotFound() : Results.NoContent();
            }
            catch (CategoryInUseException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });
    }
}
