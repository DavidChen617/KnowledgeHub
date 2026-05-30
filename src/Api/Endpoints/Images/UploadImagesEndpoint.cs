using Application.Images;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Users;

namespace Api.Endpoints.Images;

public sealed class UploadImagesEndpoint : IGroupedEndpoint<ApiImagesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .DisableAntiforgery()
            .Produces<IReadOnlyList<UploadImageResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        IFormFileCollection files,
        User currentUser,
        HttpContext ctx,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        if (files.Count == 0)
            return Results.BadRequest("No files provided.");

        var items = files.Select(f => new ImageUploadItem(f.OpenReadStream(), f.FileName)).ToList();
        var results = await dispatcher.Send(new UploadImagesCommandRequest(items), ct);

        var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
        var response = results.Select(r =>
            new UploadImageResponse(r.OriginalName, $"{baseUrl}/image/{r.StorageKey}")).ToList();

        return Results.Ok(Response.Ok(response));
    }
}

public record UploadImageResponse(string OriginalName, string Url);
