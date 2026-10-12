namespace RandomApi.Endpoints;

public static class UuidEndpoints
{
    public static IEndpointRouteBuilder MapUuidEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/random/uuid", () =>
            Results.Ok(new { uuid = Guid.NewGuid().ToString("D") }));
        return endpoints;
    }
}
