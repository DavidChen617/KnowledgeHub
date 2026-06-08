using Domain.AI;
using ShareKernal;

namespace IntegrationTests.Fakes;

public class FakeNoteStructurer : INoteStructurer
{
    public Task<Result<NoteStructureResult>> StructureAsync(string content, string userPrompt, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new NoteStructureResult(
            Description: "Fake structure",
            StructuredContent: "### 標題\n內容摘要")));
}
