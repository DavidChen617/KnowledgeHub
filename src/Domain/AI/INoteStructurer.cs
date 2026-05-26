namespace Domain.AI;

public interface INoteStructurer
{
    Task<string> StructureAsync(string content, string prompt, CancellationToken cancellationToken = default);
}
