# Soporte Tecnico (TF15) - INF-5130 UASD

Proyecto C# / .NET 10 (Domain, Application, Infrastructure, Api, Web Blazor, xUnit).

## Configuracion
1. Copiar src/SoporteTecnico.Api/appsettings.Example.json a appsettings.Development.json.
2. Migraciones:
dotnet ef database update --context SoporteDbContext --project src/SoporteTecnico.Infrastructure --startup-project src/SoporteTecnico.Api
dotnet ef database update --context AuthDbContext --project src/SoporteTecnico.Infrastructure --startup-project src/SoporteTecnico.Api

## Ejecucion y Tests
- API: dotnet run --project src/SoporteTecnico.Api --launch-profile https
- Web: dotnet run --project src/SoporteTecnico.Web
- Tests: dotnet test SoporteTecnico.slnx
