using CatalogoService.DTOs;
using CatalogoService.Mappers;
using CatalogoService.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoService.Endpoints;

public static class ProductosEndpoints
{
    // Catálogo en memoria — Lab 4 lo reemplaza con EF Core + SQL Server
    private static readonly List<Producto> _catalogo =
    [
        new() { Id = 1, Nombre = "Refresco Cola 600ml", Categoria = "Bebidas", Precio = 12.50m, Stock = 500 },
        new() { Id = 2, Nombre = "Papas Fritas 180g", Categoria = "Botanitas", Precio = 18.00m, Stock = 300 },
        new() { Id = 3, Nombre = "Agua Natural 1L", Categoria = "Bebidas", Precio = 8.00m, Stock = 800 },
        new() { Id = 4, Nombre = "Leche Entera 1L", Categoria = "Lacteos", Precio = 22.00m, Stock = 200 }
    ];

    private static int _nextId = 5;
    
    public static RouteGroupBuilder MapProductos(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/productos")
            .WithTags("Catalogo ShopFlow")
            .WithOpenApi();

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);

        return group;
    }

    static Ok<IEnumerable<ProductoDto>> GetAll(ProductoMapper mapper, string? categoria = null)
    {
        var lista = categoria is null
            ? _catalogo
            : _catalogo.Where(p => p.Categoria == categoria).ToList();

        return TypedResults.Ok(lista.Select(mapper.ToDto));
    }

    static Results<Ok<ProductoDto>, NotFound<ProblemDetails>> GetById(
        ProductoMapper mapper,
        int id
        )
    {
        Producto? p = _catalogo.FirstOrDefault(x => x.Id == id);

        return p is null
            ? TypedResults.NotFound(new ProblemDetails
            {
                Title = "Producto no encontrado",
                Detail = $"No existe en el catálogo ShopFlow un producto con ID {id}",
                Status = 404
            })
            : TypedResults.Ok(mapper.ToDto(p));
    }

    static async Task<Results<Created<ProductoDto>, ValidationProblem>> Create(
        CrearProductoDto dto, 
        IValidator<CrearProductoDto> validator,
        ProductoMapper mapper)
    {
        ValidationResult? result = await validator.ValidateAsync(dto);

        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        var p = new Producto
        {
            Id = _nextId++,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Categoria = dto.Categoria,
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        _catalogo.Add(p);

        return TypedResults.Created($"/api/v1/productos/{p.Id}", mapper.ToDto(p));
    }

    static async Task<Results<Ok<ProductoDto>, NotFound<ProblemDetails>, ValidationProblem>> Update(
        int id, 
        CrearProductoDto dto, 
        IValidator<CrearProductoDto> validator,
        ProductoMapper mapper)
    {
        ValidationResult? result = await validator.ValidateAsync(dto);

        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        Producto? p = _catalogo.FirstOrDefault(x => x.Id == id);

        if (p is null)
            return TypedResults.NotFound(new ProblemDetails
            {
                Title = "Producto no encontrado",
                Status = 404
            });

        p.Nombre = dto.Nombre;
        p.Descripcion = dto.Descripcion;
        p.Categoria = dto.Categoria;
        p.Precio = dto.Precio;
        p.Stock = dto.Stock;

        return TypedResults.Ok(mapper.ToDto(p));
    }

    static Results<NoContent, NotFound<ProblemDetails>> Delete(int id)
    {
        Producto? p = _catalogo.FirstOrDefault(x => x.Id == id);

        if (p is null)
            return TypedResults.NotFound(new ProblemDetails
            {
                Title = "Producto no encontrado",
                Status = 404
            });

        _catalogo.Remove(p);

        return TypedResults.NoContent();
    }
}