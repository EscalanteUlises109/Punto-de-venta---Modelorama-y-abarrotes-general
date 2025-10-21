using System;
using SQLite;

namespace IntegraTech_POS.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int Id_Usuario { get; set; }
        
        [NotNull, Unique]
        public string NombreUsuario { get; set; } = string.Empty;
        
        [NotNull]
        public string PasswordHash { get; set; } = string.Empty;

    
    public string PasswordAlgorithm { get; set; } = "SHA256";

    
    public string? PasswordSalt { get; set; }
        
        [NotNull]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [NotNull]
        public string Rol { get; set; } = "Cajero"; 
        
        public string Email { get; set; } = string.Empty;
        
        [NotNull]
        public bool Activo { get; set; } = true;
        
        [NotNull]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime? UltimoAcceso { get; set; }
    }
}

