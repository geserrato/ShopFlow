using OrdenesService.Events;

namespace OrdenesService.Domain;

public class Orden
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ClienteId { get; private set; } = string.Empty;
    public EstadoOrden Estado { get; private set; } = EstadoOrden.Pendiente;
    public List<OrdenItem> Items { get; private set; } = [];
    public decimal Total => Items.Sum(i => i.Precio * i.Cantidad);
    public DateTime CreadaEn { get; private set; } = DateTime.UtcNow;

    // Domain Events acumulados — se guardan en el Outbox
    private readonly List<IDomainEvent> _eventos = [];
    public IReadOnlyList<IDomainEvent> Eventos => _eventos.AsReadOnly();

    // Factory method — única forma válida de crear una Orden
    public static Orden Crear(string clienteId, List<OrdenItem> items)
    {
        var orden = new Orden { ClienteId = clienteId, Items = items };
        // El Aggregate genera su propio evento de dominio
        orden._eventos.Add(
            new OrdenCreadaEvent(orden.Id, clienteId, orden.Total));
        return orden;
    }

    public void Confirmar() => Estado = EstadoOrden.Confirmada;
    public void Cancelar() => Estado = EstadoOrden.Cancelada;
    public void LimpiarEventos() => _eventos.Clear();
}

public record OrdenItem(string ProductoId, int Cantidad, decimal Precio);

public enum EstadoOrden
{
    Pendiente,
    Confirmada,
    Cancelada
}