using Domain.Users;
using Domain.Users.Events;

namespace UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Given_ValidData_When_Create_Then_RaisesUserRegisteredEvent()
    {
        var user = User.Create("test@example.com", "testuser");

        var ev = Assert.Single(user.DomainEvents.OfType<UserRegisteredEvent>());
        Assert.Equal(user.Id.Value, ev.UserId);
        Assert.Equal("test@example.com", ev.Email);
    }

    [Fact]
    public void Given_ValidData_When_Create_Then_PropertiesAreSet()
    {
        var user = User.Create("test@example.com", "testuser", "https://avatar.url");

        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("https://avatar.url", user.AvatarUrl);
    }
}
