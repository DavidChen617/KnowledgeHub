using Domain.Shared;

namespace Domain.Users;

public class UserIdentity : Entity<Guid>
{
    public string Provider { get; }
    public string ProviderId { get; }

    private UserIdentity(Guid id, string provider, string providerId) : base(id)
    {
        Provider = provider;
        ProviderId = providerId;
    }

    public static UserIdentity Create(string provider, string providerId) =>
        new(Guid.NewGuid(), provider, providerId);
}
