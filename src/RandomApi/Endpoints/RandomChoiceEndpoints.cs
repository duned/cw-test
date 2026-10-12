namespace RandomApi.Endpoints;

public static class RandomChoiceEndpoints
{
    public static IEndpointRouteBuilder MapRandomChoiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/random/choice", (HttpRequest request) =>
        {
            var items = request.Query["items"]
                .Where(item => !string.IsNullOrEmpty(item))
                .ToArray();

            if (items.Length == 0)
            {
                return Results.BadRequest(new ApiError("At least one non-empty item is required."));
            }

            return Results.Ok(new { item = items[Random.Shared.Next(items.Length)], count = items.Length });
        });

        return endpoints;
    }
}
