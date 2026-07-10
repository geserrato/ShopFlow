// OrdenesService/Queries/GetOrdenHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdenesService.Commands;
using OrdenesService.Data;

namespace OrdenesService.Queries;

public class GetOrdenHandler(
    OrdenesDbContext db) : IRequestHandler<GetOrdenQuery, OrdenDto?>
{
    public async Task<OrdenDto?> Handle(
        GetOrdenQuery request, CancellationToken ct)
    {
        var orden = await db.Ordenes
            .FirstOrDefaultAsync(o => o.Id == request.OrdenId, ct);

        return orden is null ? null
            : new OrdenDto(orden.Id, orden.ClienteId,
                           orden.Total, orden.Estado.ToString());
    }
}
