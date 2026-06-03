using System.Net;
using System.Text.Json;

namespace ApiTests;

public class NotesTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Given_AuthenticatedUser_When_PostNote_Then_Returns201WithNoteId()
    {
        var client = factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/notes", new { title = "My Note" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.NotEqual(Guid.Empty, body.GetProperty("data").GetProperty("noteId").GetGuid());
    }

    [Fact]
    public async Task Given_ExistingNote_When_GetNoteById_Then_Returns200WithTitle()
    {
        var client = factory.CreateAuthenticatedClient();
        var created = await client.PostAsJsonAsync("/api/notes", new { title = "FindMe" });
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var noteId = createdBody.GetProperty("data").GetProperty("noteId").GetGuid();

        var response = await client.GetAsync($"/api/notes/{noteId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("FindMe", body.GetProperty("data").GetProperty("title").GetString());
    }

    [Fact]
    public async Task Given_UnknownId_When_GetNoteById_Then_Returns404()
    {
        var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/notes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Given_ExistingNote_When_PutNote_Then_Returns200WithUpdatedTitle()
    {
        var client = factory.CreateAuthenticatedClient();
        var created = await client.PostAsJsonAsync("/api/notes", new { title = "Original" });
        var noteId = (await created.Content.ReadFromJsonAsync<JsonElement>(JsonOpts))
            .GetProperty("data").GetProperty("noteId").GetGuid();

        var response = await client.PutAsJsonAsync($"/api/notes/{noteId}", new { title = "Updated" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.Equal("Updated", body.GetProperty("data").GetProperty("title").GetString());
    }

    [Fact]
    public async Task Given_ExistingNote_When_DeleteNote_Then_Returns204()
    {
        var client = factory.CreateAuthenticatedClient();
        var created = await client.PostAsJsonAsync("/api/notes", new { title = "ToDelete" });
        var noteId = (await created.Content.ReadFromJsonAsync<JsonElement>(JsonOpts))
            .GetProperty("data").GetProperty("noteId").GetGuid();

        var response = await client.DeleteAsync($"/api/notes/{noteId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Given_AuthenticatedUser_When_GetGraph_Then_Returns200WithNodesAndEdges()
    {
        var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/notes/graph");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var data = body.GetProperty("data");
        Assert.True(data.TryGetProperty("nodes", out _));
        Assert.True(data.TryGetProperty("edges", out _));
    }

    [Fact]
    public async Task Given_ExistingNote_When_PostShare_Then_Returns200WithToken()
    {
        var client = factory.CreateAuthenticatedClient();
        var created = await client.PostAsJsonAsync("/api/notes", new { title = "ShareMe" });
        var noteId = (await created.Content.ReadFromJsonAsync<JsonElement>(JsonOpts))
            .GetProperty("data").GetProperty("noteId").GetGuid();

        var response = await client.PostAsJsonAsync($"/api/notes/{noteId}/share", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.False(string.IsNullOrEmpty(body.GetProperty("data").GetProperty("url").GetString()));
    }

    [Fact]
    public async Task Given_SharedNote_When_DeleteShare_Then_Returns204()
    {
        var client = factory.CreateAuthenticatedClient();
        var created = await client.PostAsJsonAsync("/api/notes", new { title = "Shared" });
        var noteId = (await created.Content.ReadFromJsonAsync<JsonElement>(JsonOpts))
            .GetProperty("data").GetProperty("noteId").GetGuid();
        await client.PostAsJsonAsync($"/api/notes/{noteId}/share", new { });

        var response = await client.DeleteAsync($"/api/notes/{noteId}/share");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
