using System.Net;
using System.Text.Json;

namespace ApiTests;

public class CategoriesTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Given_AuthenticatedUser_When_PostCategory_Then_Returns200WithId()
    {
        var client = factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/categories", new { name = "Tech" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("data").GetProperty("name").GetString()));
    }

    [Fact]
    public async Task Given_ExistingCategory_When_GetCategories_Then_Returns200WithList()
    {
        var client = factory.CreateAuthenticatedClient();
        await client.PostAsJsonAsync("/api/categories", new { name = "Science" });

        var response = await client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var list = body.GetProperty("data").GetProperty("categories").EnumerateArray().ToList();
        Assert.Contains(list, c => c.GetProperty("name").GetString() == "Science");
    }

    [Fact]
    public async Task Given_ExistingCategory_When_PutCategory_Then_Returns200()
    {
        var client = factory.CreateAuthenticatedClient();
        var created = await client.PostAsJsonAsync("/api/categories", new { name = "OldName" });
        var catId = (await created.Content.ReadFromJsonAsync<JsonElement>(JsonOpts))
            .GetProperty("data").GetProperty("id").GetGuid();

        var response = await client.PutAsJsonAsync($"/api/categories/{catId}", new { name = "NewName" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Given_EmptyCategory_When_DeleteCategory_Then_Returns204()
    {
        var client = factory.CreateAuthenticatedClient();
        var created = await client.PostAsJsonAsync("/api/categories", new { name = "ToDelete" });
        var catId = (await created.Content.ReadFromJsonAsync<JsonElement>(JsonOpts))
            .GetProperty("data").GetProperty("id").GetGuid();

        var response = await client.DeleteAsync($"/api/categories/{catId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
