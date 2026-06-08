namespace ApiTests;

public class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Given_NoToken_When_GetNotes_Then_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/notes");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Given_NoToken_When_PostNote_Then_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/notes", new { title = "test" });

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Given_ValidToken_When_GetNotes_Then_Returns200()
    {
        var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/notes");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
