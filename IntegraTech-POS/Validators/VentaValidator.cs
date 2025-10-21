using FluentValidation;
using IntegraTech_POS.Models;

namespace IntegraTech_POS.Validators
{
    public class VentaValidator : AbstractValidator<Venta>
    {
        public VentaValidator()
        {
            RuleFor(x => x.Total)
                .GreaterThan(0).WithMessage("El total de la venta debe ser mayor a 0");
            
            RuleFor(x => x.Metodo_Pago)
                .NotEmpty().WithMessage("El método de pago es obligatorio")
                .Must(metodo => new[] { "Efectivo", "Tarjeta", "Transferencia", "Otro" }.Contains(metodo))
                .WithMessage("Método de pago inválido");
            
            RuleFor(x => x.Descuento)
                .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo")
                .LessThan(x => x.Total).WithMessage("El descuento no puede ser mayor al total");
            
            RuleFor(x => x.Impuesto)
                .GreaterThanOrEqualTo(0).WithMessage("El impuesto no puede ser negativo");
            
            RuleFor(x => x.Cliente)
                .MaximumLength(200).WithMessage("El nombre del cliente no puede exceder 200 caracteres");
        }
    }
}
