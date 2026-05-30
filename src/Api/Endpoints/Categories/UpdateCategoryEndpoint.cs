using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;
using Domain.Users;
using ShareKernal;

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
        User? currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (currentUser is null) return Results.Unauthorized();

        var result = await dispatcher.Send(new UpdateCategoryCommandRequest(new CategoryId(id), currentUser.Id, req.Name), ct);

        if (!result.IsSuccess)
            return result.Error.Type switch
            {
                ErrorType.NotFound => Results.NotFound(),
                _ => Results.Conflict(new { error = result.Error.Description })
            };

        return Results.Ok(result.Value);
    }
}

public record UpdateCategoryRequest(string Name);
