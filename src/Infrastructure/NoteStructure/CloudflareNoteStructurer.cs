namespace Infrastructure.NoteStructure;

public class CloudflareNoteStructurer(HttpClient httpClient) : NoteStructurerHandler
{
    protected override string Model => "@cf/meta/llama-3.1-8b-instruct";
    protected override HttpClient HttpClient => httpClient;
}
