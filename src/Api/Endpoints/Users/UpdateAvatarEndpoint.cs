using Api.Extensions;
using Application.Users;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using Domain.Notes;
using Domain.Users;

namespace Api.Endpoints.Users;

public sealed class UpdateAvatarEndpoint : IGroupedEndpoint<UsersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPatch("/me/avatar", HandleAsync)
            .DisableAntiforgery()
            .Produces<Response<UpdateAvatarResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        IFormFile file,
        User currentUser,
        HttpContext ctx,
        IImageStorage imageStorage,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var storageKey = await imageStorage.UploadAsync(file.OpenReadStream(), file.FileName, ct);

        var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
        var avatarUrl = $"{baseUrl}/image/{storageKey}";

        var result = await dispatcher.Send(new UpdateAvatarCommandRequest(currentUser.Id, avatarUrl), ct);
        return result.ToHttpResult(new UpdateAvatarResponse(avatarUrl));
    }
}

public record UpdateAvatarResponse(string AvatarUrl);
