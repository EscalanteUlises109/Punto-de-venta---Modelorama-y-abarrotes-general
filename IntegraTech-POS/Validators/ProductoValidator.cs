using FluentValidation;
using IntegraTech_POS.Models;

namespace IntegraTech_POS.Validators
{
    public class ProductoValidator : AbstractValidator<Producto>
    {
        public ProductoValidator()
        {
            RuleFor(x => x.Nombre_Producto)
                .NotEmpty().WithMessage("El nombre del producto es obligatorio")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");
            
            RuleFor(x => x.Precio_Venta)
                .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a 0")
                .GreaterThan(x => x.Precio_Compra).WithMessage("El precio de venta debe ser mayor al precio de compra");
            
            RuleFor(x => x.Precio_Compra)
                .GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo");
            
            RuleFor(x => x.Cantidad)
                .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa");
            
            RuleFor(x => x.Stock_Minimo)
                .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo");
            
            RuleFor(x => x.Codigo_Barras)
                .NotEmpty().WithMessage("El código de barras es obligatorio")
                .Matches(@"^[0-9]{8,13}$").WithMessage("El código de barras debe tener entre 8 y 13 dígitos")
                .When(x => !string.IsNullOrEmpty(x.Codigo_Barras));
            
            RuleFor(x => x.Fecha_Vencimiento)
                .GreaterThan(DateTime.Now).WithMessage("La fecha de vencimiento debe ser futura")
                .When(x => x.Fecha_Vencimiento.HasValue);
            
            RuleFor(x => x.Categoria)
                .NotEmpty().WithMessage("La categoría es obligatoria")
                .MaximumLength(100).WithMessage("La categoría no puede exceder 100 caracteres");
            
            RuleFor(x => x.Unidad_Medida)
                .NotEmpty().WithMessage("La unidad de medida es obligatoria")
                .MaximumLength(50).WithMessage("La unidad de medida no puede exceder 50 caracteres");
        }
    }
}
