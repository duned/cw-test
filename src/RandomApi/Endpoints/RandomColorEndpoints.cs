namespace RandomApi.Endpoints;

public static class RandomColorEndpoints
{
    public static IEndpointRouteBuilder MapRandomColorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/random/color", () =>
            Results.Ok(new { value = $"#{Random.Shared.Next(0x1000000):X6}" }));
        return endpoints;
    }
}
