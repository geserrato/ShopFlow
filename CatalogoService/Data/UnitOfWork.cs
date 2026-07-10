using CatalogoService.Repositories;

namespace CatalogoService.Data;

public class UnitOfWork(
    CatalogoDbContext db,
    IProductoRepository productos
)
{
    public IProductoRepository Productos => productos;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return db.SaveChangesAsync(ct);
    }
}