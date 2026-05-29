namespace Infrastructure.NoteStructure;

public class PollinationsNoteStructurer(HttpClient httpClient) : NoteStructurerHandler
{
    protected override string Model => "openai";
    protected override HttpClient HttpClient => httpClient;
}
