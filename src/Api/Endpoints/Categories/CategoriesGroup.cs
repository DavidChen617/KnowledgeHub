using CoreMesh.Endpoints;

namespace Api.Endpoints.Categories;

public sealed class CategoriesGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/categories";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Categories");
    }
}
