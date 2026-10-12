namespace RandomApi.Endpoints;

public static class RandomQuoteEndpoints
{
    private static readonly Quote[] Quotes =
    [
        new("descartes-think", "I think, therefore I am.", "René Descartes"),
        new("shakespeare-rose", "A rose by any other name would smell as sweet.", "William Shakespeare"),
        new("austen-truth", "It is a truth universally acknowledged, that a single man in possession of a good fortune, must be in want of a wife.", "Jane Austen"),
        new("proverb-actions", "Actions speak louder than words.", null)
    ];

    public static IEndpointRouteBuilder MapRandomQuoteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/random/quote", () =>
            Results.Ok(Quotes[Random.Shared.Next(Quotes.Length)]));
        return endpoints;
    }

    private sealed record Quote(string Id, string Text, string? Attribution);
}
