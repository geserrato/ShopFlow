using CatalogoService.Endpoints;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Integración con .NET Aspire (OpenTelemetry + Health Checks)
builder.AddServiceDefaults();

// Problem Details RFC 7807 — formato estándar de errores
builder.Services.AddProblemDetails();

// FluentValidation — registra automáticamente todos los validators del proyecto
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// OpenAPI 3.1
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapDefaultEndpoints();   // /health/live y /health/ready (Aspire)

// Scalar UI — documentación interactiva
app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.Title = "ShopFlow — Catálogo API";
    opt.Theme = ScalarTheme.BluePlanet;
});

// Registrar todos los endpoints del catálogo
app.MapProductos();

await app.RunAsync();