using MediatR;
using OrdenesService.Commands;

namespace OrdenesService.Queries;

public record GetOrdenQuery(Guid OrdenId) : IRequest<OrdenDto?>;
