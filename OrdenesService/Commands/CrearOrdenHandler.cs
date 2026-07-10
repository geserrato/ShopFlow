// OrdenesService/Commands/CrearOrdenHandler.cs
using System.Text.Json;
using MediatR;
using OrdenesService.Data;
using OrdenesService.Domain;
using OrdenesService.Outbox;

namespace OrdenesService.Commands;

public class CrearOrdenHandler(
    OrdenesDbContext db) : IRequestHandler<CrearOrdenCommand, OrdenDto>
{
    public async Task<OrdenDto> Handle(
        CrearOrdenCommand request, CancellationToken ct)
    {
        // 1. Crear el Aggregate con Domain Events
        var items = request.Items
            .Select(i => new OrdenItem(i.ProductoId, i.Cantidad, i.Precio))
            .ToList();
        var orden = Orden.Crear(request.ClienteId, items);

        // 2. Persistir la orden
        db.Ordenes.Add(orden);

        // 3. Outbox Pattern: guardar eventos en la MISMA transacción
        foreach (var evento in orden.Eventos)
        {
            db.Outbox.Add(new OutboxMessage
            {
                Tipo = evento.GetType().Name,
                Payload = JsonSerializer.Serialize(evento, evento.GetType()),
            });
        }

        // 4. Un solo SaveChanges = orden + outbox en una transacción atómica
        await db.SaveChangesAsync(ct);
        orden.LimpiarEventos();

        return new OrdenDto(
            orden.Id, orden.ClienteId,
            orden.Total, orden.Estado.ToString());
    }
}
