using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Confluent.Kafka;
using CoreMesh.Outbox.Abstractions;
using Domain.Notes;
using Domain.NoteStructure;
using Domain.Notifications;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace ApiTests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecret = "test-jwt-secret-key-for-api-tests-at-least-32-chars!";
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Secret", TestJwtSecret);
        builder.UseSetting("ConnectionStrings:Default", "");
        builder.UseSetting("ConnectionStrings:Redis", "");

        builder.ConfigureServices(services =>
        {
            // Replace DB with InMemory TestDbContext
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            var testOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(_dbName)
                .Options;
            services.AddScoped<AppDbContext>(_ => new TestDbContext(testOptions));

            // Remove Kafka hosted services
            services.RemoveAll<IHostedService>();

            // Remove Redis
            services.RemoveAll<IConnectionMultiplexer>();
            services.RemoveAll<IDatabase>();
            services.RemoveAll<IServer>();
            services.RemoveAll<ICacher>();
            services.RemoveAll<IStructureRateLimiter>();
            services.AddSingleton<ICacher, FakeCacher>();
            services.AddScoped<IStructureRateLimiter, FakeStructureRateLimiter>();

            // Remove Kafka producer/consumer and outbox messaging
            services.RemoveAll<IProducer<string, string>>();
            services.RemoveAll<IConsumer<string, string>>();
            services.RemoveAll<IEventPublisher>();
            services.RemoveAll<IMessageSubscriber>();
            services.AddSingleton<IEventPublisher, FakeEventPublisher>();
            services.AddSingleton<IMessageSubscriber, FakeMessageSubscriber>();

            // Replace AI providers
            services.RemoveAll<INoteStructurer>();
            services.RemoveAll<IEmbedder>();
            services.RemoveAll<IImageDescriber>();
            services.AddScoped<INoteStructurer, FakeNoteStructurer>();
            services.AddScoped<IEmbedder, FakeEmbedder>();
            services.AddScoped<IImageDescriber, FakeImageDescriber>();

            // Replace email and storage
            services.RemoveAll<IEmailSender>();
            services.RemoveAll<IImageStorage>();
            services.AddScoped<IEmailSender, FakeEmailSender>();
            services.AddScoped<IImageStorage, FakeImageStorage>();

            // Remove Cloudinary
            services.RemoveAll<CloudinaryDotNet.Cloudinary>();
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var user = User.Create("test@example.com", "testuser");

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(user);
        db.SaveChanges();

        var token = GenerateToken(user.Id.Value);
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
