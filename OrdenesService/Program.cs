// OrdenesService/Program.cs
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// Órdenes de prueba — se reemplaza por CQRS + EF Core en Lab 3
app.MapGet("/ordenes", () => Results.Ok(new[]
{
    new { Id=1, ClienteId="dist-mx-001", Estado="Confirmada", Total=450.00m },
    new { Id=2, ClienteId="dist-mx-002", Estado="Pendiente", Total=280.00m },
}));

await app.RunAsync();