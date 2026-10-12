using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using RandomApi.Endpoints;

namespace RandomApi.Tests;

public sealed class RandomChoiceEndpointTests : IAsyncLifetime
{
    private WebApplication app = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        app = builder.Build();
        app.MapRandomChoiceEndpoints();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    [Theory]
    [InlineData("?items=red", "red")]
    [InlineData("?items=%20Red%20", " Red ")]
    [InlineData("?items=%20", " ")]
    [InlineData("?items=caf%C3%A9%20%26%20blue%2Bgreen", "café & blue+green")]
    [InlineData("?items=&items=red&items=", "red")]
    public async Task OneValidItemReturnsOriginalText(string query, string expected)
    {
        using var response = await client.GetAsync("/api/random/choice" + query);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(expected, json.RootElement.GetProperty("item").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("count").GetInt32());
    }

    [Theory]
    [InlineData("?items=red&items=blue", 2)]
    [InlineData("?items=red&items=red&items=blue", 3)]
    [InlineData("?items=&items=red&items=blue&items=", 2)]
    public async Task MultipleItemsReturnSuppliedCandidateAndCount(string query, int count)
    {
        using var response = await client.GetAsync("/api/random/choice" + query);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(json.RootElement.GetProperty("item").GetString(), new[] { "red", "blue" });
        Assert.Equal(count, json.RootElement.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task DuplicateItemsRemainSeparateCandidates()
    {
        using var response = await client.GetAsync("/api/random/choice?items=red&items=red");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("red", json.RootElement.GetProperty("item").GetString());
        Assert.Equal(2, json.RootElement.GetProperty("count").GetInt32());
    }

    [Theory]
    [InlineData("")]
    [InlineData("?items")]
    [InlineData("?items=")]
    [InlineData("?items=&items=")]
    [InlineData("?other=red")]
    public async Task NoValidItemsReturnSharedErrorShape(string query)
    {
        using var response = await client.GetAsync("/api/random/choice" + query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Single(json.RootElement.EnumerateObject());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("error").GetString()));
    }
}
