using Microsoft.EntityFrameworkCore;
using OrdenesService.Data;

namespace OrdenesService.Outbox;

// Hosted Service que drena la tabla Outbox.
// Corre cada 5 segundos y publica los mensajes pendientes.
// Es el patrón "Transactional Outbox": la app escribe el mensaje
// en la misma transacción que el cambio de dominio, y este worker
// se encarga de enviarlo al broker (Service Bus, RabbitMQ, etc.).
public class OutboxWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxWorker> logger) : BackgroundService
{
    // ExecuteAsync es el entry point del BackgroundService.
    // Se ejecuta UNA vez al arrancar, y el método debe "mantenerse vivo"
    // hasta que el host se apague (de ahí el while + ct).
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Bucle principal: mientras nadie nos cancele, procesamos y esperamos.
        // Task.Delay respeta el token, así que si el host se detiene
        // salimos limpios en lugar de quedar colgados.
        while (!ct.IsCancellationRequested)
        {
            await ProcesarOutbox(ct);
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }

    // Un "tick" del worker: lee un lote, lo procesa y persiste.
    private async Task ProcesarOutbox(CancellationToken ct)
    {
        // Scope propio — BackgroundService tiene vida Singleton,
        // pero el DbContext es Scoped. Sin este scope no podríamos
        // resolverlo ni garantizar que se liberen sus recursos al final.
        using var scope = scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<OrdenesDbContext>();

        // Traemos hasta 10 mensajes pendientes por tick.
        // El .Take(10) limita el lote para no bloquear la tabla
        // y dejar margen a otros workers / réplicas en el futuro.
        var pendientes = await db.Outbox
            .Where(m => m.ProcesadoEn == null)
            .Take(10)
            .ToListAsync(ct);

        // Nada que hacer → salimos temprano. Evita logs ruidosos
        // y un SaveChanges innecesario.
        if (!pendientes.Any()) return;

        foreach (var msg in pendientes)
        {
            // Lab 5: aquí se reemplaza el log por publicación a Service Bus.
            // Hoy solo "fingimos" la publicación; mañana va el SendMessage
            // real contra el broker.
            logger.LogInformation(
                "[ShopFlow Outbox] Publicando {Tipo}: {Payload}",
                msg.Tipo, msg.Payload);

            // Marcamos el mensaje como procesado en memoria.
            // El UPDATE real sale con el SaveChangesAsync de abajo,
            // así todos los updates van en una sola transacción.
            msg.ProcesadoEn = DateTime.UtcNow;
        }

        // Persistimos todos los ProcesadoEn en un único INSERT/UPDATE batch.
        // Si la publicación real fallara en el futuro, acá habría que
        // invertir el orden: publicar PRIMERO, marcar DESPUÉS,
        // y mover la marca a otro SaveChanges dentro de un try/catch.
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "[ShopFlow Outbox] {Count} mensaje(s) procesados.", pendientes.Count);
    }
}