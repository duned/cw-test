namespace RandomApi.Endpoints;

public static class RandomBooleanEndpoints
{
    public static IEndpointRouteBuilder MapRandomBooleanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/random/boolean", () =>
            Results.Ok(new { value = Random.Shared.Next(2) == 1 }));
        return endpoints;
    }
}
