using System.Net;
using System.Net.Mail;
using Domain.Notifications;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Notifications;

public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = configuration["Smtp:Host"]!;
        var port = int.Parse(configuration["Smtp:Port"]!);
        var username = configuration["Smtp:Username"]!;
        var password = configuration["Smtp:Password"]!;
        var from = configuration["Smtp:From"]!;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(username, password)
        };

        using var message = new MailMessage(from, to, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message, ct);
    }
}
