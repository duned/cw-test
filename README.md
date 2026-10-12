# Random API scaffold

Minimal ASP.NET Core API targeting .NET 10. Install the .NET 10 SDK; `global.json`
allows installed .NET 10 feature bands (developed with SDK 10.0.112).

From the repository root:

```sh
dotnet restore RandomApi.slnx
dotnet build RandomApi.slnx --no-restore
dotnet run --project src/RandomApi --no-launch-profile --urls http://localhost:5000
```

Check the running application:

```sh
curl http://localhost:5000/health
```

`GET /health` returns HTTP 200 with `{"status":"ok"}`.

Run the tests:

```sh
dotnet test RandomApi.slnx
```

The test project uses xUnit and an in-memory ASP.NET Core application host.
The API has no third-party runtime packages.

Add future routes as separate `Map...Endpoints` extension modules under
`src/RandomApi/Endpoints`, following `MapHealthEndpoints`. Register modules in
`Program.cs`; final endpoint integration belongs to issue 1. No random-data
endpoints are included in this scaffold.

Request-binding failures (such as malformed JSON, invalid typed parameters, or
unsupported body content types) return `{"error":"Invalid request input."}` with
HTTP 400 or 415. Binding exceptions are enabled in every environment and handled
by the input-error middleware without exposing parser details. Future endpoints
can return the shared `ApiError` record for their own validation failures.
