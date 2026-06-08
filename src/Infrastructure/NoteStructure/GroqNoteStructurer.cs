namespace Infrastructure.NoteStructure;

public class GroqNoteStructurer(HttpClient httpClient) : NoteStructurerHandler
{
    protected override string Model => "llama-3.3-70b-versatile";
    protected override HttpClient HttpClient => httpClient;
}
