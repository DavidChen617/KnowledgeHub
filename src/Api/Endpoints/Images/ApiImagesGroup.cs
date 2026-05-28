using CoreMesh.Endpoints;

namespace Api.Endpoints.Images;

public sealed class ApiImagesGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/images";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Images").RequireAuthorization();
    }
}
