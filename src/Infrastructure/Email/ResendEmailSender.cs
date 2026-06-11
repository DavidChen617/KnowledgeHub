using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Domain.Notifications;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Email;

public sealed class ResendEmailSender(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var apiKey = configuration["Resend:ApiKey"]!;
        var from = configuration["Resend:From"]!;

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = JsonSerializer.Serialize(new
        {
            from,
            to = new[] { to },
            subject,
            html = htmlBody
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.resend.com/emails", content, ct);
        response.EnsureSuccessStatusCode();
    }
}
