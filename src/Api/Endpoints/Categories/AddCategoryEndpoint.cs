using Api.Extensions;
using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Exceptions;

namespace Api.Endpoints.Categories;

public sealed class AddCategoryEndpoint : IGroupedEndpoint<CategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .Produces<AddCategoryCommandResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        AddCategoryRequest req,
        IDispatcher dispatcher,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!ctx.TryGetUserId(out var userId))
            return Results.Unauthorized();

        try
        {
            var result = await dispatcher.Send(new AddCategoryCommandRequest(userId, req.Name), ct);
            return Results.Created($"/api/categories/{result.CategoryId}", result);
        }
        catch (DuplicateCategoryNameException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }
}

public record AddCategoryRequest(string Name);
