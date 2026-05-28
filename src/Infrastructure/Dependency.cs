using System.Net.Http.Headers;
using System.Reflection;
using Confluent.Kafka;
using CoreMesh.Outbox.Extensions;
using Domain.AI;
using Domain.Categories;
using Domain.Notes;
using Domain.Shared;
using Infrastructure.Cloudinary;
using Infrastructure.Embedding;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Kafka;
using Infrastructure.NoteStructure;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
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
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<INoteSearcher, NoteSearcher>();
            services.AddSingleton(_ => new CloudinaryDotNet.Cloudinary(new CloudinaryDotNet.Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            )) { Api = { Secure = true } });

            services.AddScoped<IImageStorage, CloudinaryImageStorage>();

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

            services.AddHttpClient<INoteStructurer, GroqNoteStructurer>(client =>
            {
                client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Groq:ApiKey"]);
            });

            services.AddHttpClient<IEmbedder, CohereEmbedder>(client =>
            {
                client.BaseAddress = new Uri("https://api.cohere.com/v2/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration["Cohere:ApiKey"]);
            });

            return services;
        }
    }
}
