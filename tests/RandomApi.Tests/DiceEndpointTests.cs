using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using RandomApi.Endpoints;

namespace RandomApi.Tests;

public sealed class DiceEndpointTests : IAsyncLifetime
{
    private WebApplication app = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        app = builder.Build();
        app.MapDiceEndpoints();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    [Theory]
    [InlineData("", 1, 6)]
    [InlineData("?count=12", 12, 6)]
    [InlineData("?sides=20", 1, 20)]
    [InlineData("?count=8&sides=20", 8, 20)]
    [InlineData("?count=5&sides=1", 5, 1)]
    [InlineData("?count=100&sides=1000000", 100, 1_000_000)]
    public async Task RollsHaveRequestedDimensionsRangeAndTotal(string query, int count, int sides)
    {
        using var response = await client.GetAsync("/api/random/dice" + query);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var body = json.RootElement;
        Assert.Equal(count, body.GetProperty("count").GetInt32());
        Assert.Equal(sides, body.GetProperty("sides").GetInt32());
        var rolls = body.GetProperty("rolls").EnumerateArray().Select(roll => roll.GetInt32()).ToArray();
        Assert.Equal(count, rolls.Length);
        Assert.All(rolls, roll => Assert.InRange(roll, 1, sides));
        Assert.Equal(rolls.Sum(), body.GetProperty("total").GetInt32());
        if (sides == 1)
        {
            Assert.Equal(count, body.GetProperty("total").GetInt32());
        }
    }

    [Theory]
    [InlineData("count=0")]
    [InlineData("count=-1")]
    [InlineData("count=101")]
    [InlineData("count=2147483648")]
    [InlineData("count=abc")]
    [InlineData("count=1.5")]
    [InlineData("count=")]
    [InlineData("count=1&count=2")]
    [InlineData("sides=0")]
    [InlineData("sides=-1")]
    [InlineData("sides=1000001")]
    [InlineData("sides=2147483648")]
    [InlineData("sides=abc")]
    [InlineData("sides=1.5")]
    [InlineData("sides=")]
    [InlineData("sides=6&sides=8")]
    public async Task InvalidParametersReturnSharedErrorShape(string query)
    {
        using var response = await client.GetAsync("/api/random/dice?" + query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Single(json.RootElement.EnumerateObject());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("error").GetString()));
    }
}
