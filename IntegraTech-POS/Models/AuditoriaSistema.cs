using System;
using SQLite;

namespace IntegraTech_POS.Models
{
    [Table("AuditoriaSistema")]
    public class AuditoriaSistema
    {
        [PrimaryKey, AutoIncrement]
        public int Id_Auditoria { get; set; }
        
        [NotNull]
        public int Id_Usuario { get; set; }
        
        [NotNull]
        public string NombreUsuario { get; set; } = string.Empty;
        
        [NotNull]
        public string Accion { get; set; } = string.Empty; 
        
        public string Detalles { get; set; } = string.Empty;
        
        public string TablaAfectada { get; set; } = string.Empty;
        
        public int? IdRegistroAfectado { get; set; }
        
        [NotNull]
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        public string DireccionIP { get; set; } = string.Empty;
    }
}

