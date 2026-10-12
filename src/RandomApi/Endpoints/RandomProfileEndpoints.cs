namespace RandomApi.Endpoints;

public static class RandomProfileEndpoints
{
    private static readonly string[] Adjectives = ["Silver", "Sunny", "Cosmic", "Velvet", "Amber", "Jolly"];
    private static readonly string[] Creatures = ["Otter", "Dragon", "Fox", "Griffin", "Owl", "Phoenix"];

    public static IEndpointRouteBuilder MapRandomProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/random/profile", () =>
        {
            var adjective = Adjectives[Random.Shared.Next(Adjectives.Length)];
            var creature = Creatures[Random.Shared.Next(Creatures.Length)];
            var username = $"mock-{adjective.ToLowerInvariant()}-{creature.ToLowerInvariant()}-{Random.Shared.Next(1000, 10000)}";

            return Results.Ok(new
            {
                displayName = $"Mock {adjective} {creature}",
                username,
                email = $"{username}@example.com"
            });
        });
        return endpoints;
    }
}
