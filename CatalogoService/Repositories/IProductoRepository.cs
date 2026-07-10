using CatalogoService.Models;

namespace CatalogoService.Repositories;

public interface IProductoRepository
{
    Task<Producto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Producto>> GetAllAsync(string? categoria = null, CancellationToken ct = default);
    Task AddAsync(Producto p, CancellationToken ct = default);
    void Update(Producto p);
    void Delete(Producto p);
    Task<IEnumerable<ResumenCategoriaDto>> GetResumenPorCategoriaAsync();
}

public record ResumenCategoriaDto(
    string Categoria,
    int TotalProductos,
    decimal PrecioMinimo,
    decimal PrecioMaximo,
    decimal PrecioPromedio);