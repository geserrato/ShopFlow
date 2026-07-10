// OrdenesService/Program.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdenesService.Commands;
using OrdenesService.Data;
using OrdenesService.Outbox;
using OrdenesService.Queries;
using ShopFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Integración con .NET Aspire
builder.AddServiceDefaults();

// EF Core in-memory (Lab 4 reemplaza esto con SQL Server)
builder.Services.AddDbContext<OrdenesDbContext>(
    opt => opt.UseInMemoryDatabase("ShopFlowOrdenes"));

// MediatR — registra automáticamente todos los handlers del proyecto
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<Program>());

// OutboxWorker — se ejecuta en segundo plano
builder.Services.AddHostedService<OutboxWorker>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenApi();

// WRITE — Command dispatched via MediatR
app.MapPost("/ordenes", async (
    CrearOrdenCommand cmd, IMediator mediator) =>
{
    var result = await mediator.Send(cmd);
    return Results.Created($"/ordenes/{result.Id}", result);
});

// READ — Query dispatched via MediatR
app.MapGet("/ordenes/{id:guid}", async (
    Guid id, IMediator mediator) =>
{
    var orden = await mediator.Send(new GetOrdenQuery(id));
    return orden is null ? Results.NotFound() : Results.Ok(orden);
});

// Simular confirmación por Saga (coreografía)
// En Lab 5 este endpoint se reemplaza por un consumer de Service Bus
app.MapPost("/ordenes/{id:guid}/confirmar", async (
    Guid id, OrdenesDbContext db) =>
{
    var orden = await db.Ordenes.FindAsync(id);
    if (orden is null) return Results.NotFound();

    orden.Confirmar();
    await db.SaveChangesAsync();

    return Results.Ok(new { mensaje = "Orden confirmada", estado = "Confirmada" });
});

await app.RunAsync();