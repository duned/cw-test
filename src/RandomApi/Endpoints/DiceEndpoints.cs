using System.Globalization;

namespace RandomApi.Endpoints;

public static class DiceEndpoints
{
    public static IEndpointRouteBuilder MapDiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/random/dice", (HttpRequest request) =>
        {
            if (!TryReadParameter(request, "count", 1, 100, out var count))
            {
                return Results.BadRequest(new ApiError("count must be an integer between 1 and 100."));
            }

            if (!TryReadParameter(request, "sides", 6, 1_000_000, out var sides))
            {
                return Results.BadRequest(new ApiError("sides must be an integer between 1 and 1000000."));
            }

            var rolls = new int[count];
            var total = 0;
            for (var index = 0; index < rolls.Length; index++)
            {
                rolls[index] = Random.Shared.Next(1, sides + 1);
                total += rolls[index];
            }

            return Results.Ok(new { count, sides, rolls, total });
        });

        return endpoints;
    }

    private static bool TryReadParameter(
        HttpRequest request, string name, int defaultValue, int maximum, out int value)
    {
        value = defaultValue;
        if (!request.Query.TryGetValue(name, out var input))
        {
            return true;
        }

        return input.Count == 1 &&
            int.TryParse(input[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
            value >= 1 && value <= maximum;
    }
}
