using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using RandomApi.Endpoints;

namespace RandomApi.Tests;

public sealed class RandomNumberEndpointTests : IAsyncLifetime
{
    private WebApplication app = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
        app = builder.Build();
        app.UseMiddleware<RequestInputErrorMiddleware>();
        app.MapRandomNumberEndpoints();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    [Theory]
    [InlineData("", 0, 100)]
    [InlineData("?min=20", 20, 100)]
    [InlineData("?max=20", 0, 20)]
    [InlineData("?min=-10&max=10", -10, 10)]
    [InlineData("?min=0&max=1", 0, 1)]
    [InlineData("?min=-2147483648&max=2147483647", int.MinValue, int.MaxValue)]
    public async Task ValuesStayWithinInclusiveBounds(string query, int min, int max)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            using var response = await client.GetAsync("/api/random/number" + query);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.InRange(json.RootElement.GetProperty("value").GetInt32(), min, max);
            Assert.Equal(min, json.RootElement.GetProperty("min").GetInt32());
            Assert.Equal(max, json.RootElement.GetProperty("max").GetInt32());
        }
    }

    [Theory]
    [InlineData(42)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task SingleValueRangeReturnsItsBound(int bound)
    {
        using var response = await client.GetAsync($"/api/random/number?min={bound}&max={bound}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(bound, json.RootElement.GetProperty("value").GetInt32());
        Assert.Equal(bound, json.RootElement.GetProperty("min").GetInt32());
        Assert.Equal(bound, json.RootElement.GetProperty("max").GetInt32());
    }

    [Theory]
    [InlineData("?min=10&max=9")]
    [InlineData("?min=101")]
    [InlineData("?max=-1")]
    [InlineData("?min=-2147483649&max=0")]
    [InlineData("?min=0&max=2147483648")]
    [InlineData("?min=not-a-number")]
    [InlineData("?max=1.5")]
    public async Task InvalidBoundsReturnConsistentError(string query)
    {
        using var response = await client.GetAsync("/api/random/number" + query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Invalid request input.", json.RootElement.GetProperty("error").GetString());
        Assert.Single(json.RootElement.EnumerateObject());
    }
}
