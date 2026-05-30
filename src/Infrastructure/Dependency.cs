using System.Net.Http.Headers;
using System.Reflection;
using Confluent.Kafka;
using CoreMesh.Outbox.Extensions;
using Domain.AI;
using Domain.Categories;
using Domain.Comments;
using Domain.Notifications;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using Infrastructure.Auth;
using Infrastructure.FileStore;
using Infrastructure.Embedding;
using Infrastructure.ImageDescription;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Kafka;
using Infrastructure.NoteStructure;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Notifications;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class Dependency
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration, params Assembly[] handlerAssemblies)
        {
            services.AddSingleton<DomainEventInterceptor>();

            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<INoteSearcher, NoteSearcher>();
            services.AddScoped<IIdentityProvider, GoogleIdentityProvider>();
            services.AddScoped<ITokenIssuer, JwtTokenIssuer>();
            services.AddSingleton(_ => new CloudinaryDotNet.Cloudinary(new CloudinaryDotNet.Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            )) { Api = { Secure = true } });

            services.AddScoped<IImageStorage, CloudinaryImageStorage>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("Default"),
                    o => o.UseVector());
                options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
            });

            var bootstrapServers = configuration["Kafka:BootstrapServers"]!;
            var groupId = configuration["Kafka:GroupId"] ?? "knowledge-hub";
            
            services.AddSingleton<IProducer<string, string>>(_ =>
                new ProducerBuilder<string, string>(new ProducerConfig
                {
                    BootstrapServers = bootstrapServers,
                    Acks = Acks.All
                }).Build());

            services.AddSingleton<IConsumer<string, string>>(_ =>
                new ConsumerBuilder<string, string>(new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = groupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                }).Build());

            services.AddHostedService<KafkaTopicInitializer>();

            services.AddCoreMeshOutbox(
                [.. handlerAssemblies, typeof(Domain.Notes.Events.NoteDeletedEvent).Assembly],
                options =>
                {
                    options.AddOutboxStore<EfCoreOutboxStore>()
                           .AddOutboxWriter<EfCoreOutboxWriter>()
                           .AddMessageQueue<KafkaEventPublisher, KafkaMessageSubscriber>()
                           .WithConsumer();
                });

            services.AddHttpClient<GeminiImageDescriber>(client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
                client.DefaultRequestHeaders.Add("x-goog-api-key", configuration["Gemini:ApiKey"]!);
            });
            services.AddHttpClient<GroqImageDescriber>(client =>
            {
                client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Groq:ApiKey"]);
            });
            services.AddHttpClient<MistralImageDescriber>(client =>
            {
                client.BaseAddress = new Uri("https://api.mistral.ai/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Mistral:ApiKey"]);
            });
            services.AddHttpClient<OpenRouterImageDescriber>(client =>
            {
                client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["OpenRouter:ApiKey"]);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "https://knowledge-hub.local");
            });
            services.AddScoped<IImageDescriber>(sp =>
            {
                var describers = new ImageDescriberHandler[]
                {
                    sp.GetRequiredService<GeminiImageDescriber>(),
                    sp.GetRequiredService<GroqImageDescriber>(),
                    sp.GetRequiredService<MistralImageDescriber>(),
                    sp.GetRequiredService<OpenRouterImageDescriber>(),
                };

                var shuffled = describers.OrderBy(_ => Random.Shared.Next()).ToArray();

                for (var i = 0; i < shuffled.Length - 1; i++)
                    shuffled[i].SetNext(shuffled[i + 1]);

                return shuffled[0];
            });

            services.AddHttpClient("ImageDownloader");
            services.AddHttpClient<GroqNoteStructurer>(client =>
            {
                client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Groq:ApiKey"]);
            });
            services.AddHttpClient<MistralNoteStructurer>(client =>
            {
                client.BaseAddress = new Uri("https://api.mistral.ai/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Mistral:ApiKey"]);
            });
            services.AddHttpClient<CerebrasNoteStructurer>(client =>
            {
                client.BaseAddress = new Uri("https://api.cerebras.ai/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Cerebras:ApiKey"]);
            });
            services.AddHttpClient<OpenRouterNoteStructurer>(client =>
            {
                client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["OpenRouter:ApiKey"]);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "https://knowledge-hub.local");
            });
            services.AddHttpClient<CloudflareNoteStructurer>(client =>
            {
                var accountId = configuration["Cloudflare:AccountId"]!;
                client.BaseAddress = new Uri($"https://api.cloudflare.com/client/v4/accounts/{accountId}/ai/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Cloudflare:ApiToken"]);
            });
            services.AddHttpClient<PollinationsNoteStructurer>(client =>
            {
                client.BaseAddress = new Uri("https://text.pollinations.ai/openai/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Pollinations:ApiKey"]);
            });
            services.AddScoped<INoteStructurer>(sp =>
            {
                var structurers = new NoteStructurerHandler[]
                {
                    sp.GetRequiredService<GroqNoteStructurer>(),
                    sp.GetRequiredService<MistralNoteStructurer>(),
                    sp.GetRequiredService<CerebrasNoteStructurer>(),
                    sp.GetRequiredService<OpenRouterNoteStructurer>(),
                    sp.GetRequiredService<CloudflareNoteStructurer>(),
                    sp.GetRequiredService<PollinationsNoteStructurer>(),
                };

                var shuffled = structurers.OrderBy(_ => Random.Shared.Next()).ToArray();

                for (var i = 0; i < shuffled.Length - 1; i++)
                    shuffled[i].SetNext(shuffled[i + 1]);

                return shuffled[0];
            });

            services.AddHttpClient<CohereEmbedder>(client =>
            {
                client.BaseAddress = new Uri("https://api.cohere.com/v2/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Cohere:ApiKey"]);
            });
            services.AddHttpClient<MistralEmbedder>(client =>
            {
                client.BaseAddress = new Uri("https://api.mistral.ai/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Mistral:ApiKey"]);
            });
            services.AddHttpClient<CloudflareEmbedder>(client =>
            {
                var accountId = configuration["Cloudflare:AccountId"]!;
                client.BaseAddress = new Uri($"https://api.cloudflare.com/client/v4/accounts/{accountId}/ai/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Cloudflare:ApiToken"]);
            });
            services.AddHttpClient<OpenRouterEmbedder>(client =>
            {
                client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["OpenRouter:ApiKey"]);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "https://knowledge-hub.local");
            });
            services.AddHttpClient<GeminiEmbedder>(client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
                client.DefaultRequestHeaders.Add("x-goog-api-key", configuration["Gemini:ApiKey"]!);
            });
            services.AddScoped<IEmbedder>(sp =>
            {
                var embedders = new EmbedderHandler[]
                {
                    sp.GetRequiredService<CohereEmbedder>(),
                    sp.GetRequiredService<MistralEmbedder>(),
                    sp.GetRequiredService<CloudflareEmbedder>(),
                    sp.GetRequiredService<OpenRouterEmbedder>(),
                    sp.GetRequiredService<GeminiEmbedder>(),
                };

                var shuffled = embedders.OrderBy(_ => Random.Shared.Next()).ToArray();

                for (var i = 0; i < shuffled.Length - 1; i++)
                    shuffled[i].SetNext(shuffled[i + 1]);

                return shuffled[0];
            });

            return services;
        }
    }
}
