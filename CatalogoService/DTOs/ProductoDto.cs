namespace CatalogoService.DTOs;

public record ProductoDto( int Id, string Nombre, string Categoria, decimal Precio, int Stock, bool Activo);