using Api.Extensions;
using CoreMesh.Endpoints;

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

        var encodedPublicId = string.Join('/',
            publicId.Split('/').Select(Uri.EscapeDataString));
        var cloudinaryUrl = $"https://res.cloudinary.com/{cloudName}/image/upload/{encodedPublicId}";
        return Results.Redirect(cloudinaryUrl);
    }
}
