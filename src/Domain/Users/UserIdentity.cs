using Domain.Shared;

namespace Domain.Users;

public class UserIdentity : Entity<UserIdentityId>
{
    public UserId UserId { get; }
    public string Provider { get; }
    public string ProviderSub { get; }

    private UserIdentity(UserIdentityId id, UserId userId, string provider, string providerSub) : base(id)
    {
        UserId = userId;
        Provider = provider;
        ProviderSub = providerSub;
    }

    public static UserIdentity Create(UserId userId, string provider, string providerSub) =>
        new(new UserIdentityId(Guid.NewGuid()), userId, provider, providerSub);
}
