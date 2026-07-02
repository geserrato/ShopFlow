using Asp.Versioning;
using Asp.Versioning.Builder;
using CatalogoService.Endpoints;
using CatalogoService.Mappers;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Integración con .NET Aspire (OpenTelemetry + Health Checks)
builder.AddServiceDefaults();
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(opt =>
{
    opt.GroupNameFormat = "'v'V";
    opt.SubstituteApiVersionInUrl = true;
});

// Problem Details RFC 7807 — formato estándar de errores
builder.Services.AddProblemDetails();

// Mapperly 
builder.Services.AddSingleton<ProductoMapper>();

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

ApiVersionSet versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .Build();

app.MapProductos().WithApiVersionSet(versionSet);

await app.RunAsync();