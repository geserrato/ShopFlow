using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogoService.Data;

public class CatalogoDbContextFactory : IDesignTimeDbContextFactory<CatalogoDbContext>
{
    public CatalogoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogoDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=ShopFlowCatalogo;Username=postgres;Password=postgres123");
        return new CatalogoDbContext(optionsBuilder.Options);
    }
}