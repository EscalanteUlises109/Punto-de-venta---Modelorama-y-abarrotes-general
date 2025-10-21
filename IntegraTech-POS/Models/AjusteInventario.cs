using System;
using SQLite;

namespace IntegraTech_POS.Models
{
    [Table("AjustesInventario")]
    public class AjusteInventario
    {
        [PrimaryKey, AutoIncrement]
        public int Id_Ajuste { get; set; }
        
        [NotNull]
        public int Id_Producto { get; set; }
        
        [NotNull]
        public int CantidadAnterior { get; set; }
        
        [NotNull]
        public int CantidadNueva { get; set; }
        
        [NotNull]
        public int CantidadAjuste { get; set; } 
        
        [NotNull]
        public string TipoAjuste { get; set; } = string.Empty; 
        
        [NotNull]
        public string Motivo { get; set; } = string.Empty;
        
        [NotNull]
        public int Id_Usuario { get; set; }
        
        [NotNull]
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        public string Observaciones { get; set; } = string.Empty;
    }
}

