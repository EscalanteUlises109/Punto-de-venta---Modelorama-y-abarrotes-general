using FluentValidation;
using IntegraTech_POS.Models;

namespace IntegraTech_POS.Validators
{
    public class UsuarioValidator : AbstractValidator<Usuario>
    {
        public UsuarioValidator()
        {
            RuleFor(x => x.NombreUsuario)
                .NotEmpty().WithMessage("El nombre de usuario es obligatorio")
                .MinimumLength(4).WithMessage("El nombre de usuario debe tener al menos 4 caracteres")
                .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder 50 caracteres")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("El nombre de usuario solo puede contener letras, nÃºmeros y guiÃ³n bajo");
            
            RuleFor(x => x.NombreCompleto)
                .NotEmpty().WithMessage("El nombre completo es obligatorio")
                .MaximumLength(200).WithMessage("El nombre completo no puede exceder 200 caracteres");
            
            RuleFor(x => x.Rol)
                .NotEmpty().WithMessage("El rol es obligatorio")
                .Must(rol => new[] { "Admin", "Gerente", "Cajero" }.Contains(rol))
                .WithMessage("Rol invÃ¡lido. Debe ser: Admin, Gerente o Cajero");
            
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El formato del email no es vÃ¡lido")
                .When(x => !string.IsNullOrEmpty(x.Email));
            
            
            RuleFor(x => x.PasswordHash)
                .NotEmpty().WithMessage("La contraseÃ±a es obligatoria")
                .Must(hash => !string.IsNullOrEmpty(hash) && (hash.Length >= 44))
                .WithMessage("Hash de contraseÃ±a invÃ¡lido");
        }
    }
}

