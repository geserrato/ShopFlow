namespace ShopFlow.Contracts;

public class StockReservadoEvent
{
    public Guid OrdenId { get; init; }
    public bool Exitoso { get; init; }
    public string? Razon { get; init; }
}