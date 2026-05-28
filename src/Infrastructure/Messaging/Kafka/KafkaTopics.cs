namespace Infrastructure.Messaging.Kafka;

internal static class KafkaTopics
{
    public const string NoteDeleted = "note.deleted";
    public const string NoteImagesChanged = "note.images-changed";

    public static string FromEventType(string eventType) => eventType switch
    {
        "note.deleted" => NoteDeleted,
        "note.images-changed" => NoteImagesChanged,
        _ => throw new InvalidOperationException($"Unknown event type: {eventType}")
    };
}
