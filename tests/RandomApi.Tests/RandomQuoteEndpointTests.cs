using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using RandomApi.Endpoints;

namespace RandomApi.Tests;

public sealed class RandomQuoteEndpointTests
{
    private static readonly Dictionary<string, (string Text, string? Attribution)> ExpectedQuotes = new()
    {
        ["descartes-think"] = ("I think, therefore I am.", "René Descartes"),
        ["shakespeare-rose"] = ("A rose by any other name would smell as sweet.", "William Shakespeare"),
        ["austen-truth"] = ("It is a truth universally acknowledged, that a single man in possession of a good fortune, must be in want of a wife.", "Jane Austen"),
        ["proverb-actions"] = ("Actions speak louder than words.", null)
    };

    [Fact]
    public async Task RandomQuoteReturnsJsonWithStableIdTextAndAttributionFromCollection()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        Assert.Same(app, app.MapRandomQuoteEndpoints());
        await app.StartAsync();

        using var client = app.GetTestClient();
        // Validate each result without requiring any particular selection or distribution.
        for (var request = 0; request < 50; request++)
        {
            using var response = await client.GetAsync("/api/random/quote");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var quote = body.RootElement;
            Assert.Equal(JsonValueKind.Object, quote.ValueKind);
            Assert.Equal(3, quote.EnumerateObject().Count());

            var id = quote.GetProperty("id");
            Assert.Equal(JsonValueKind.String, id.ValueKind);
            Assert.False(string.IsNullOrWhiteSpace(id.GetString()));
            Assert.True(ExpectedQuotes.TryGetValue(id.GetString()!, out var expected));

            var text = quote.GetProperty("text");
            Assert.Equal(JsonValueKind.String, text.ValueKind);
            Assert.False(string.IsNullOrWhiteSpace(text.GetString()));
            Assert.Equal(expected.Text, text.GetString());

            var attribution = quote.GetProperty("attribution");
            Assert.Equal(expected.Attribution is null ? JsonValueKind.Null : JsonValueKind.String,
                attribution.ValueKind);
            Assert.Equal(expected.Attribution, attribution.GetString());
        }
    }
}
