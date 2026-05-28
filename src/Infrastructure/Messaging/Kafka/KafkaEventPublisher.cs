using Confluent.Kafka;
using CoreMesh.Outbox.Abstractions;

namespace Infrastructure.Messaging.Kafka;

internal sealed class KafkaEventPublisher(IProducer<string, string> producer) : IEventPublisher
{
    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        var topic = KafkaTopics.FromEventType(message.EventType);
        await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = message.EventType,
            Value = message.Payload,
            Timestamp = new Timestamp(message.OccurredAtUtc)
        }, cancellationToken);
    }
}
