using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using RandomApi.Endpoints;

namespace RandomApi.Tests;

public sealed class RandomProfileEndpointTests
{
    [Fact]
    public async Task RandomProfileReturnsStableShapeWithFictionalFields()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.MapRandomProfileEndpoints();
        await app.StartAsync();

        using var client = app.GetTestClient();
        for (var i = 0; i < 10; i++)
        {
            using var response = await client.GetAsync("/api/random/profile");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var profile = body.RootElement;
            Assert.Equal(JsonValueKind.Object, profile.ValueKind);
            Assert.Equal(3, profile.EnumerateObject().Count());

            foreach (var propertyName in new[] { "displayName", "username", "email" })
            {
                var field = profile.GetProperty(propertyName);
                Assert.Equal(JsonValueKind.String, field.ValueKind);
                Assert.False(string.IsNullOrWhiteSpace(field.GetString()));
            }

            Assert.StartsWith("Mock ", profile.GetProperty("displayName").GetString()!);
            var username = profile.GetProperty("username").GetString()!;
            Assert.Matches("^mock-[a-z]+-[a-z]+-[0-9]{4}$", username);
            var email = profile.GetProperty("email").GetString()!;
            Assert.Equal("example.com", email.Split('@').Last());
            Assert.Equal($"{username}@example.com", email);
        }
    }
}
