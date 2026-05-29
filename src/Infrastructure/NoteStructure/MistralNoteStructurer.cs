namespace Infrastructure.NoteStructure;

public class MistralNoteStructurer(HttpClient httpClient) : NoteStructurerHandler
{
    protected override string Model => "mistral-small-latest";
    protected override HttpClient HttpClient => httpClient;
}
