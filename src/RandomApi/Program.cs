using RandomApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteHandlerOptions>(options =>
    options.ThrowOnBadRequest = true);

var app = builder.Build();
app.UseMiddleware<RandomApi.RequestInputErrorMiddleware>();
app.MapHealthEndpoints();
app.MapRandomPingEndpoints();
app.Run();

// Exposes the entry point to the integration test host.
public partial class Program;
