namespace OrdenesService.Events;

public interface IDomainEvent { }

public record OrdenCreadaEvent(
    Guid     OrdenId,
    string   ClienteId,
    decimal Total
) : IDomainEvent;