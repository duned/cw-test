namespace RandomApi;

public sealed record ApiError(string Error);

public sealed class RequestInputErrorMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException exception) when (
            !context.Response.HasStarted &&
            exception.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status415UnsupportedMediaType)
        {
            context.Response.Clear();
            context.Response.StatusCode = exception.StatusCode;
            await context.Response.WriteAsJsonAsync(new ApiError("Invalid request input."));
        }
    }
}
