using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RandomApi.Tests;

public sealed class RequestInputErrorMiddlewareTests
{
    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status415UnsupportedMediaType)]
    public async Task InvalidInputReturnsSmallJsonError(int statusCode)
    {
        var context = new DefaultHttpContext();
        using var body = new MemoryStream();
        context.Response.Body = body;
        var middleware = new RequestInputErrorMiddleware(_ =>
            throw new BadHttpRequestException("Internal binding details", statusCode));

        await middleware.InvokeAsync(context);

        Assert.Equal(statusCode, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);
        body.Position = 0;
        using var json = await JsonDocument.ParseAsync(body);
        Assert.Equal("Invalid request input.", json.RootElement.GetProperty("error").GetString());
        Assert.Single(json.RootElement.EnumerateObject());
    }

    [Fact]
    public async Task UnexpectedErrorsPropagate()
    {
        var exception = new InvalidOperationException("Unexpected failure");
        var middleware = new RequestInputErrorMiddleware(_ => throw exception);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            middleware.InvokeAsync(new DefaultHttpContext()));

        Assert.Same(exception, actual);
    }
}
