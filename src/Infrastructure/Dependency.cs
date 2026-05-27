using System.Net.Http.Headers;
using Domain.AI;
using Infrastructure.Embedding;
using Infrastructure.NoteStructure;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class Dependency
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services.AddSingleton<DomainEventInterceptor>();

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("Default"));
                options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
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
