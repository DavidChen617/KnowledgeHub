using Domain.Shared;

namespace Domain.Users;

public class User : AggregateRoot<UserId>
{
    public string Email { get; private set; }
    public string Username { get; private set; }

    private User(UserId id, string email, string username) : base(id)
    {
        Email = email;
        Username = username;
    }

    public static User Create(string email, string username) =>
        new(new UserId(Guid.NewGuid()), email, username);
}
