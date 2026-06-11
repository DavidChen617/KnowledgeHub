using Api.Extensions;
using Application.Categories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Users;

namespace Api.Endpoints.Categories;

public sealed class AddCategoryEndpoint : IGroupedEndpoint<CategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .Produces<Response<AddCategoryDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        AddCategoryRequest req,
        User currentUser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new AddCategoryCommand(currentUser.Id, req.Name), ct);
        return result.ToCreated(v => $"/api/categories/{v.Id}");
    }
}

public record AddCategoryRequest(string Name);
