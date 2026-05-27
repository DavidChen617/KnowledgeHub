using Domain.Users;

namespace Api.Extensions;

public static class HttpContextExtensions
{
    extension(HttpContext ctx)
    {
        public bool TryGetUserId(out UserId userId)
        {
            if (ctx.Request.Headers.TryGetValue("X-User-Id", out var value) &&
                Guid.TryParse(value, out var guid))
            {
                userId = new UserId(guid);
                return true;
            }

            userId = null!;
            return false;
        }
    }
}
