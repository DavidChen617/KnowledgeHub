using Domain.Users;

namespace UnitTests.Users;

public class UserTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var user = User.Create("test@example.com", "Test User", "https://avatar.url");
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("Test User", user.Name);
    }

    [Fact]
    public void AddIdentity_AddsToIdentities()
    {
        var user = User.Create("test@example.com", "Test User");
        user.AddIdentity("google", "google-sub-123");

        Assert.Single(user.Identities);
        Assert.Equal("google", user.Identities[0].Provider);
        Assert.Equal("google-sub-123", user.Identities[0].ProviderId);
    }

    [Fact]
    public void IssueRefreshToken_AddsToRefreshTokens()
    {
        var user = User.Create("test@example.com", "Test User");
        user.IssueRefreshToken("token-abc");

        Assert.Single(user.RefreshTokens);
        Assert.False(user.RefreshTokens[0].IsExpired);
    }

    [Fact]
    public void RevokeRefreshToken_RemovesToken()
    {
        var user = User.Create("test@example.com", "Test User");
        user.IssueRefreshToken("token-abc");
        user.RevokeRefreshToken("token-abc");

        Assert.Empty(user.RefreshTokens);
    }

    [Fact]
    public void RevokeRefreshToken_OnlyRemovesMatchingToken()
    {
        var user = User.Create("test@example.com", "Test User");
        user.IssueRefreshToken("token-1");
        user.IssueRefreshToken("token-2");
        user.RevokeRefreshToken("token-1");

        Assert.Single(user.RefreshTokens);
        Assert.Equal("token-2", user.RefreshTokens[0].Token);
    }
}
