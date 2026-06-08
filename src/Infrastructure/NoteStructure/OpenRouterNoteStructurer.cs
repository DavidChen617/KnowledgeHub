namespace Infrastructure.NoteStructure;

public class OpenRouterNoteStructurer(HttpClient httpClient) : NoteStructurerHandler
{
    protected override string Model => "openai/gpt-oss-20b:free";
    protected override HttpClient HttpClient => httpClient;
}
