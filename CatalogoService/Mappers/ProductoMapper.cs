using CatalogoService.DTOs;
using CatalogoService.Models;
using Riok.Mapperly.Abstractions;

namespace CatalogoService.Mappers;

[Mapper]
public partial class ProductoMapper
{
    public partial ProductoDto ToDto(Producto producto);
    public partial Producto ToEntity(CrearProductoDto dto);
}