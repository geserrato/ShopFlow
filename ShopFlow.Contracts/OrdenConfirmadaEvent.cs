namespace ShopFlow.Contracts;

public class OrdenConfirmadaEvent
{
    public Guid OrdenId { get; init; }
    public string ClienteId { get; init; } = string.Empty;
    public decimal Total { get; init; }
}