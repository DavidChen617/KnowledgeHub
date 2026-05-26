namespace Domain.AI;

public static class Chunker
{
    public static IReadOnlyList<(int Index, T Artifact)> Chunk<T>(string content, Func<string, IReadOnlyList<(int, T)>> mapper) =>
        mapper(content);
}
