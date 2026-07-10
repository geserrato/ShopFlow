namespace ShopFlow.Contracts;

public class OrdenCreadaEvent
{
    public Guid OrdenId { get; init; }
    public string ClienteId { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public DateTime OcurridoEn { get; init; } = DateTime.UtcNow;
}