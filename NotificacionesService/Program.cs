// NotificacionesService/Program.cs

using ShopFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
WebApplication app = builder.Build();
app.MapDefaultEndpoints();
// Stub — en Lab 5 este endpoint se convierte en consumer de Service Bus
app.MapPost("/notificaciones/enviar", (object payload) =>
{
    Console.WriteLine($"[ShopFlow Notif] Evento recibido: {payload}");
    return Results.Accepted();
}).WithOpenApi();

await app.RunAsync();
