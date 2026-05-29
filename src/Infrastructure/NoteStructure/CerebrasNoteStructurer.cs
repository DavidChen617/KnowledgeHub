namespace Infrastructure.NoteStructure;

public class CerebrasNoteStructurer(HttpClient httpClient) : NoteStructurerHandler
{
    protected override string Model => "gpt-oss-120b";
    protected override HttpClient HttpClient => httpClient;
}
