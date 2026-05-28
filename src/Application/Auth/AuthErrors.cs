using ShareKernal;

namespace Application.Auth;

public static class AuthErrors
{
    public static readonly Error InvalidToken = new("Auth.InvalidToken", "Invalid or expired token", ErrorType.Forbidden);
}
