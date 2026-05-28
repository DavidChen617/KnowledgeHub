using CoreMesh.Endpoints;

namespace Api.Endpoints.Images;

public sealed class ImageProxyGroup : IGroupEndpoint
{
    public string GroupPrefix => "/image";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Images");
    }
}
