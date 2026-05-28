using Domain.Shared;

namespace Domain.Users;

public sealed class UserId : ValueObject
{
    public Guid Value { get; }
    public UserId(Guid value) => Value = value;
    public static UserId New() => new(Guid.NewGuid());
    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
}

public class User : AggregateRoot<UserId>
{
    public string Email { get; private set; }
    public string Username { get; private set; }
    public string? AvatarUrl { get; private set; }

    private User(UserId id, string email, string username, string? avatarUrl) : base(id)
    {
        Email = email;
        Username = username;
        AvatarUrl = avatarUrl;
    }

    public static User Create(string email, string username, string? avatarUrl = null) =>
        new(UserId.New(), email, username, avatarUrl);

    public void UpdateAvatar(string? avatarUrl) => AvatarUrl = avatarUrl;
}
