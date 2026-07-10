using MediatR;

namespace OrdenesService.Commands;

public record OrdenItemDto(string ProductoId, int Cantidad, decimal Precio);

public record OrdenDto(Guid Id, string ClienteId, decimal Total, string Estado);

public record CrearOrdenCommand(
    string ClienteId,
    List<OrdenItemDto> Items
) : IRequest<OrdenDto>;
