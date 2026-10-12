namespace RandomApi.Endpoints;

public static class RandomNumberEndpoints
{
    public static IEndpointRouteBuilder MapRandomNumberEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/random/number", (int? min, int? max) =>
        {
            var lowerBound = min ?? 0;
            var upperBound = max ?? 100;

            if (lowerBound > upperBound)
            {
                return Results.BadRequest(new ApiError("Invalid request input."));
            }

            // Widen before adding one so the inclusive maximum and full int range are safe.
            var value = (int)Random.Shared.NextInt64(lowerBound, (long)upperBound + 1);
            return Results.Ok(new { value, min = lowerBound, max = upperBound });
        });

        return endpoints;
    }
}
