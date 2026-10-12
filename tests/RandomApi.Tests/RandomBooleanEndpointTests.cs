using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using RandomApi.Endpoints;

namespace RandomApi.Tests;

public sealed class RandomBooleanEndpointTests
{
    [Fact]
    public async Task RandomBooleanReturnsOkWithJsonBooleanValue()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.MapRandomBooleanEndpoints();
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var response = await client.GetAsync("/api/random/boolean");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Object, body.RootElement.ValueKind);
        Assert.Single(body.RootElement.EnumerateObject());
        Assert.Contains(body.RootElement.GetProperty("value").ValueKind,
            new[] { JsonValueKind.True, JsonValueKind.False });
    }
}
