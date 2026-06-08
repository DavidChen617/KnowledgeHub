using Domain.Notifications;

namespace IntegrationTests.Fakes;

public class FakeEmailSender : IEmailSender
{
    public record SentEmail(string To, string Subject, string HtmlBody);

    public List<SentEmail> SentEmails { get; } = [];

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        SentEmails.Add(new SentEmail(to, subject, htmlBody));
        return Task.CompletedTask;
    }
}
