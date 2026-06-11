using Application.EventHandlers;
using Domain.Notifications;
using Domain.Users.Events;

namespace UnitTests.Users;

public class WelcomeEmailHandlerTests
{
    [Fact]
    public async Task Given_UserRegisteredEvent_When_Handle_Then_SendsWelcomeEmail()
    {
        var sender = new FakeEmailSender();
        var handler = new WelcomeEmailHandler(sender);
        var @event = new UserRegisteredEvent(Guid.NewGuid(), "newuser@example.com");

        await handler.HandleAsync(@event);

        Assert.Single(sender.SentEmails);
        Assert.Equal("newuser@example.com", sender.SentEmails[0].To);
    }
}

file sealed class FakeEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> SentEmails { get; } = [];

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        SentEmails.Add((to, subject, htmlBody));
        return Task.CompletedTask;
    }
}
