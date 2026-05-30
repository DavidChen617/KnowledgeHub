using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Kafka;

internal sealed class KafkaTopicInitializer(
    IProducer<string, string> producer,
    ILogger<KafkaTopicInitializer> logger) : IHostedService
{
    private static readonly string[] Topics = [KafkaTopics.DomainEvents];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var adminClient = new DependentAdminClientBuilder(producer.Handle).Build();

        var specs = Topics.Select(t => new TopicSpecification
        {
            Name = t,
            NumPartitions = 1,
            ReplicationFactor = 1
        }).ToList();

        try
        {
            await adminClient.CreateTopicsAsync(specs);
            logger.LogInformation("Kafka topics created: {Topics}", string.Join(", ", Topics));
        }
        catch (CreateTopicsException ex)
        {
            var errors = ex.Results.Where(r => r.Error.IsError && r.Error.Code != ErrorCode.TopicAlreadyExists);
            if (errors.Any())
                throw;

            logger.LogInformation("Kafka topics already exist, skipping creation");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
