using Api.Extensions;
using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;
using Domain.Exceptions;

namespace Api.Endpoints.Categories;

public sealed class UpdateCategoryEndpoint : IGroupedEndpoint<CategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", HandleAsync)
            .Produces<UpdateCategoryCommandResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateCategoryRequest req,
        IDispatcher dispatcher,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!ctx.TryGetUserId(out var userId))
            return Results.Unauthorized();

        try
        {
            var result = await dispatcher.Send(new UpdateCategoryCommandRequest(new CategoryId(id), userId, req.Name), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }
        catch (DuplicateCategoryNameException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }
}

public record UpdateCategoryRequest(string Name);
