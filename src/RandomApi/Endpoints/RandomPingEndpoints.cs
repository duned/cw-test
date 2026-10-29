namespace RandomApi.Endpoints;

public static class RandomPingEndpoints
{
    public static IEndpointRouteBuilder MapRandomPingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/random/ping", () => Results.Ok(new { message = "pong" }));
        return endpoints;
    }
}
