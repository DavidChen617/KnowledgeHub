using Domain.Shared;

namespace Domain.Users;

public class User : AggregateRoot<UserId>
{
    private readonly List<UserIdentity> _identities = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    public string Email { get; private set; }
    public string Name { get; private set; }
    public string? AvatarUrl { get; private set; }

    public IReadOnlyList<UserIdentity> Identities => _identities;
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens;

    private User(UserId id, string email, string name, string? avatarUrl) : base(id)
    {
        Email = email;
        Name = name;
        AvatarUrl = avatarUrl;
    }

    public static User Create(string email, string name, string? avatarUrl = null) =>
        new(UserId.New(), email, name, avatarUrl);

    public void AddIdentity(string provider, string providerId) =>
        _identities.Add(UserIdentity.Create(provider, providerId));

    public RefreshToken IssueRefreshToken(string token) =>
        _refreshTokens.AddAndReturn(RefreshToken.Create(token));

    public void RevokeRefreshToken(string token) =>
        _refreshTokens.RemoveAll(t => t.Token == token);
}

file static class ListExtensions
{
    public static T AddAndReturn<T>(this List<T> list, T item)
    {
        list.Add(item);
        return item;
    }
}
