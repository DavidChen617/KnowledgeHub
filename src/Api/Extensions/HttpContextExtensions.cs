using Domain.Users;

namespace Api.Extensions;

public static class HttpContextExtensions
{
    public static bool TryGetUserId(this HttpContext ctx, out UserId userId)
    {
        if (ctx.Request.Headers.TryGetValue("X-User-Id", out var value) &&
            Guid.TryParse(value, out var guid))
        {
            userId = new UserId(guid);
            return true;
        }

        userId = default!;
        return false;
    }
}
