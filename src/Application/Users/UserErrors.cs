using ShareKernal;

namespace Application.Users;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "User not found", ErrorType.NotFound);
}
