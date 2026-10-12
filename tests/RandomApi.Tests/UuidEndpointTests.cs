using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using RandomApi.Endpoints;

namespace RandomApi.Tests;

public sealed class UuidEndpointTests
{
    [Fact]
    public async Task UuidReturnsOkWithCanonicalUuidJson()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.MapUuidEndpoints();
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var response = await client.GetAsync("/api/random/uuid");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Object, body.RootElement.ValueKind);
        Assert.Single(body.RootElement.EnumerateObject());
        var uuid = body.RootElement.GetProperty("uuid");
        Assert.Equal(JsonValueKind.String, uuid.ValueKind);
        var value = uuid.GetString();
        Assert.True(Guid.TryParseExact(value, "D", out var parsed));
        Assert.Equal(parsed.ToString("D"), value);
    }
}
