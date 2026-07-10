using Microsoft.EntityFrameworkCore;
using OrdenesService.Domain;
using OrdenesService.Outbox;

namespace OrdenesService.Data;

public class OrdenesDbContext(DbContextOptions<OrdenesDbContext> options) : DbContext(options)
{
    public DbSet<Orden> Ordenes => Set<Orden>();


    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Orden>(o =>
        {
            o.HasKey(x => x.Id);
            // OrdenItem es parte de Orden — no tiene tabla propia, es un Owned Entity o ValueObject en terminologia de DDD
            o.OwnsMany(x => x.Items, item =>
            {
                item.WithOwner();
                item.Property(i => i.ProductoId);
                item.Property(i => i.Cantidad);
                item.Property(i => i.Precio);
            });
        });
    }
}