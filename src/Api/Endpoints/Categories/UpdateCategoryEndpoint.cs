using Api.Extensions;
using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Categories;
using Domain.Users;

namespace Api.Endpoints.Categories;

public sealed class UpdateCategoryEndpoint : IGroupedEndpoint<CategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", HandleAsync)
            .Produces<Response<UpdateCategoryCommandResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateCategoryRequest req,
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new UpdateCategoryCommandRequest(new CategoryId(id), currentUser.Id, req.Name), ct);

        return result.ToHttpResult();
    }
}

public record UpdateCategoryRequest(string Name);
