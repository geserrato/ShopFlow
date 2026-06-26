using CatalogoService.DTOs;
using FluentValidation;

namespace CatalogoService.Validators;

public class CrearProductoValidator : AbstractValidator<CrearProductoDto>
{
    private static readonly string[] _categorias =
        ["Bebidas", "Botanitas", "Lacteos", "Abarrotes", "Limpieza"];

    public CrearProductoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio")
            .MaximumLength(120).WithMessage("Máximo 120 caracteres");

        RuleFor(x => x.Categoria)
            .NotEmpty().WithMessage("La categoría es obligatoria")
            .Must(c => _categorias.Contains(c))
            .WithMessage("Categoría inválida. Permitidas: Bebidas, Botanitas, Lacteos, Abarrotes, Limpieza");

        RuleFor(x => x.Precio)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a $0.00")
            .LessThanOrEqualTo(99999).WithMessage("El precio no puede superar $99,999.00");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");
    }
}