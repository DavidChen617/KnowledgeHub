using Confluent.Kafka;
using CoreMesh.Outbox.Abstractions;

namespace Infrastructure.Messaging.Kafka;

internal sealed class KafkaEventPublisher(IProducer<string, string> producer) : IEventPublisher
{
    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await producer.ProduceAsync(KafkaTopics.DomainEvents, new Message<string, string>
        {
            Key = message.EventType,
            Value = message.Payload,
            Timestamp = new Timestamp(message.OccurredAtUtc)
        }, cancellationToken);
    }
}
