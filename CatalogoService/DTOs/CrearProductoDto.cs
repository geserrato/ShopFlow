namespace CatalogoService.DTOs;

public record CrearProductoDto(string Nombre, string Descripcion, string Categoria, decimal Precio, int Stock);