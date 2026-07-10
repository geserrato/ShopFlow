using System.Data;
using CatalogoService.Data;
using CatalogoService.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace CatalogoService.Repositories;

public class ProductoRepository(CatalogoDbContext db, IDbConnection dapper) : IProductoRepository
{
    // EF Core: escrituras con change tracking
    public Task<Producto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return db.Productos.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IEnumerable<Producto>> GetAllAsync(
        string? categoria, CancellationToken ct)
    {
        var q = db.Productos.AsNoTracking();
        if (categoria is not null)
            q = q.Where(p => p.Categoria == categoria);
        return await q.ToListAsync(ct);
    }

    public async Task AddAsync(Producto p, CancellationToken ct)
    {
        await db.Productos.AddAsync(p, ct);
    }

    public void Update(Producto p)
    {
        db.Productos.Update(p);
    }

    public void Delete(Producto p)
    {
        db.Productos.Remove(p);
    }

    // Dapper: reporte sin overhead de EF Core
    public async Task<IEnumerable<ResumenCategoriaDto>> GetResumenPorCategoriaAsync()
    {
        const string sql = """
                           SELECT Categoria,
                           COUNT(*) AS TotalProductos,
                           MIN(Precio) AS PrecioMinimo,
                           MAX(Precio) AS PrecioMaximo,
                           AVG(Precio) AS PrecioPromedio
                           FROM Productos
                           WHERE Activo = 1
                           GROUP BY Categoria
                           ORDER BY Categoria
                           """;
        return await dapper.QueryAsync<ResumenCategoriaDto>(sql);
    }
}