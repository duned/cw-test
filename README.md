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

`MapRandomQuoteEndpoints` defines `GET /api/random/quote`, returning HTTP 200
with JSON such as `{"id":"descartes-think","text":"I think, therefore I am.","attribution":"René Descartes"}`.
Each request selects from four built-in quotes without calling an external
service. IDs remain stable; attribution is `null` when unknown. The module is
tested in an isolated host; issue 1 will register it in `Program.cs`.

`MapRandomProfileEndpoints` defines `GET /api/random/profile`, returning HTTP 200
with a stable JSON shape such as `{"displayName":"Mock Silver Otter","username":"mock-silver-otter-1234","email":"mock-silver-otter-1234@example.com"}`.
Profiles use only local fictional samples, and all emails use `example.com`.
The module is tested in an isolated host; issue 1 will register it in `Program.cs`.

Add future routes as separate `Map...Endpoints` extension modules under
`src/RandomApi/Endpoints`, following `MapHealthEndpoints`. Register modules in
`Program.cs`; final endpoint integration belongs to issue 1.

`MapUuidEndpoints` defines `GET /api/random/uuid`, returning HTTP 200 with
`{"uuid":"123e4567-e89b-42d3-a456-426614174000"}` (example value). The `uuid`
property is a newly generated random UUID string in canonical lowercase
`xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` format. The module is tested in an isolated
host and is not yet registered in `Program.cs`; issue 1 will wire it into the app.

The dice module exposes `MapDiceEndpoints()` for integration by issue 1.
Once registered, `GET /api/random/dice?count=2&sides=6` returns JSON such as
`{"count":2,"sides":6,"rolls":[3,5],"total":8}`. Omitted parameters default
to `count=1` and `sides=6`. Each roll is in the inclusive range `1..sides`.
Count must be an integer from 1 to 100, and sides must be an integer from 1
to 1,000,000. These limits bound allocation and work per request and keep the
maximum total (100,000,000) within a 32-bit integer. Invalid, empty, repeated,
or out-of-range parameters return HTTP 400 with the shared `{"error":"..."}`
shape and a message describing the valid range. The module is tested in its
own in-memory host without changing the application entry point.

Request-binding failures (such as malformed JSON, invalid typed parameters, or
unsupported body content types) return `{"error":"Invalid request input."}` with
HTTP 400 or 415. Binding exceptions are enabled in every environment and handled
by the input-error middleware without exposing parser details. Future endpoints
can return the shared `ApiError` record for their own validation failures.

`MapRandomChoiceEndpoints()` defines `GET /api/random/choice?items=red&items=blue`,
returning JSON such as `{"item":"red","count":2}`. Empty values are ignored;
non-empty values retain their text, including whitespace. Duplicate values count
as separate candidates, and each candidate has an equal chance of selection.
Missing items or only empty values return HTTP 400 with the shared `{"error":"..."}`
shape. The module is tested in an isolated host; issue 1 will register it in `Program.cs`.
