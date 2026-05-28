using System.Security.Claims;
using Domain.Users;

namespace Api.Extensions;

public static class HttpContextExtensions
{
    extension(HttpContext ctx)
    {
        public bool TryGetUserId(out UserId userId)
        {
            var sub = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sub is not null && Guid.TryParse(sub, out var guid))
            {
                userId = new UserId(guid);
                return true;
            }

            userId = null!;
            return false;
        }
    }
}
