using Api.Extensions;
using CoreMesh.Endpoints;
using Microsoft.Extensions.Configuration;

namespace Api.Endpoints.Images;

public sealed class ImageRedirectEndpoint : IGroupedEndpoint<ImageProxyGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{**publicId}", HandleAsync)
            .Produces(StatusCodes.Status302Found)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();
    }

    private static IResult HandleAsync(string publicId, IConfiguration config)
    {
        var cloudName = config["Cloudinary:CloudName"];
        if (string.IsNullOrEmpty(cloudName))
            return ResultExtensions.NotFound();

        var cloudinaryUrl = $"https://res.cloudinary.com/{cloudName}/image/upload/{publicId}";
        return Results.Redirect(cloudinaryUrl);
    }
}
