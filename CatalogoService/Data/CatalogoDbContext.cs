using CatalogoService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoService.Data;

public class CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Producto>(p =>
        {
            p.HasKey(x => x.Id);
            p.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            p.Property(x => x.Categoria).HasMaxLength(50).IsRequired();
            p.Property(x => x.Precio).HasColumnType("decimal(18,2)");
            p.HasIndex(x => x.Nombre);
        });
    }

    // Auto-actualiza ActualizadoEn en cada modificación
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var e in ChangeTracker.Entries<Producto>()
                     .Where(x => x.State == EntityState.Modified))
            e.Entity.ActualizadoEn = DateTime.UtcNow;
        return base.SaveChangesAsync(ct);
    }
}