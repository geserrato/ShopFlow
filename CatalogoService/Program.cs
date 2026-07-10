using System.Data;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using CatalogoService.Auth;
using CatalogoService.Data;
using CatalogoService.Endpoints;
using CatalogoService.Mappers;
using CatalogoService.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;
using ShopFlow.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
// ── Versionado (del Lab 2) ────────────────────────────────────────
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

// ── EF Core 10 con Postgress ─────────────────────────────────────
builder.Services.AddDbContext<CatalogoDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repository + Unit of Work ─────────────────────────────────────
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<UnitOfWork>();

// ── Dapper: misma conexión que EF Core ───────────────────────────
builder.Services.AddTransient<IDbConnection>(_ =>
    new NpgsqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT Authentication ────────────────────────────────────────────
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<TokenRevocationStore>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
        opt.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                var jti = ctx.Principal!.FindFirst("jti")?.Value!;
                var store = ctx.HttpContext.RequestServices
                    .GetRequiredService<TokenRevocationStore>();
                if (store.EstaRevocado(jti)) ctx.Fail("Token revocado");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("SoloAdmin",
        p => p.RequireRole(JwtTokenService.RolAdmin));
    opts.AddPolicy("Autenticado",
        p => p.RequireAuthenticatedUser());
});

// ── Rate Limiting nativo .NET 10 ─────────────────────────────────
builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = 429;
    opts.AddTokenBucketLimiter("login", o =>
    {
        o.TokenLimit = 5;
        o.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        o.TokensPerPeriod = 3;
        o.AutoReplenishment = true;
    });
    opts.AddFixedWindowLimiter("catalogo", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 60;
    });
});

// ── Otros servicios ───────────────────────────────────────────────
builder.Services.AddSingleton<ProductoMapper>(); // Mapperly (Lab 2)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.Title = "ShopFlow — Catálogo API (con seguridad)";
    opt.Theme = ScalarTheme.BluePlanet;
});

// ── Endpoint de login (público, rate limited) ─────────────────────
app.MapPost("/auth/login", (LoginRequest req, JwtTokenService svc) =>
    {
        var (userId, rol) = req.Email switch
        {
            "admin@shopflow.mx" => ("u-admin-001", JwtTokenService.RolAdmin),
            "dist@shopflow.mx" => ("u-dist-001", JwtTokenService.RolDistribuidor),
            _ => (null, null)
        };
        if (userId is null) return Results.Unauthorized();
        var token = svc.EmitirToken(userId, req.Email, rol!);
        var refresh = svc.EmitirRefreshToken();
        return Results.Ok(new { accessToken = token, refreshToken = refresh });
    })
    .RequireRateLimiting("login")
    .AllowAnonymous();

// ── Catálogo con autenticación y versionado ───────────────────────
ApiVersionSet versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .Build();

app.MapProductos()
    .WithApiVersionSet(versionSet)
    .RequireAuthorization("Autenticado")
    .RequireRateLimiting("catalogo");

// ── Reporte por categoría — solo Admin ───────────────────────────
app.MapGet("/catalogo/reporte", async (IProductoRepository repo) =>
        Results.Ok(await repo.GetResumenPorCategoriaAsync()))
    .RequireAuthorization("SoloAdmin");

// ── Logout ───────────────────────────────────────────────────────
app.MapPost("/auth/logout", (
    HttpContext ctx, TokenRevocationStore store) =>
{
    var jti = ctx.User.FindFirst("jti")?.Value;
    if (jti is not null) store.Revocar(jti);
    return Results.NoContent();
}).RequireAuthorization();

await app.RunAsync();

internal record LoginRequest(string Email, string Password);