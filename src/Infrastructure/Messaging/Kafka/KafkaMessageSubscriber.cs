using System.Runtime.CompilerServices;
using Confluent.Kafka;
using CoreMesh.Outbox.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Kafka;

internal sealed class KafkaMessageSubscriber(
    IConsumer<string, string> consumer,
    IProducer<string, string> producer,
    ILogger<KafkaMessageSubscriber> logger) : IMessageSubscriber
{
    private static readonly string[] Topics =
    [
        KafkaTopics.NoteDeleted,
        KafkaTopics.NoteImagesChanged,
    ];

    public async IAsyncEnumerable<EventEnvelope> SubscribeAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        consumer.Subscribe(Topics);

        while (!cancellationToken.IsCancellationRequested)
        {
            var cr = consumer.Consume(cancellationToken);
            var timestamp = cr.Message.Timestamp.UtcDateTime == default
                ? DateTime.UtcNow
                : cr.Message.Timestamp.UtcDateTime;

            var headers = new Dictionary<string, string>
            {
                ["kafka-topic"] = cr.Topic,
                ["kafka-partition"] = cr.Partition.Value.ToString(),
                ["kafka-offset"] = cr.Offset.Value.ToString()
            };

            yield return EventEnvelope.Create(cr.Message.Key, cr.Message.Value, timestamp, headers);
        }

        await Task.CompletedTask;
    }

    public Task AckAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        if (TryParseTopicPartitionOffset(envelope, out var tpo))
            consumer.Commit([new TopicPartitionOffset(tpo.Topic, tpo.Partition, tpo.Offset + 1)]);
        return Task.CompletedTask;
    }

    public async Task NackAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        var dlt = $"{envelope.Type}.DLT";
        try
        {
            await producer.ProduceAsync(dlt, new Message<string, string>
            {
                Key = envelope.Type,
                Value = envelope.Payload,
                Timestamp = new Timestamp(envelope.OccurredAtUtc)
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send event '{EventType}' to DLT '{DLT}'", envelope.Type, dlt);
        }

        if (TryParseTopicPartitionOffset(envelope, out var tpo))
            consumer.Commit([new TopicPartitionOffset(tpo.Topic, tpo.Partition, tpo.Offset + 1)]);
    }

    public Task RetryAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        if (TryParseTopicPartitionOffset(envelope, out var tpo))
            consumer.Seek(new TopicPartitionOffset(tpo.Topic, tpo.Partition, tpo.Offset));
        return Task.CompletedTask;
    }

    private static bool TryParseTopicPartitionOffset(EventEnvelope envelope, out (string Topic, int Partition, long Offset) result)
    {
        if (envelope.Headers.TryGetValue("kafka-topic", out var topic) &&
            envelope.Headers.TryGetValue("kafka-partition", out var partitionStr) &&
            envelope.Headers.TryGetValue("kafka-offset", out var offsetStr) &&
            int.TryParse(partitionStr, out var partition) &&
            long.TryParse(offsetStr, out var offset))
        {
            result = (topic, partition, offset);
            return true;
        }
        result = default;
        return false;
    }
}
