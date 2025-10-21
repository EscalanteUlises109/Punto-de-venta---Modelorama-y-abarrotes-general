using System;
using SQLite;

namespace IntegraTech_POS.Models
{
    [Table("DevolucionesVenta")]
    public class DevolucionVenta
    {
        [PrimaryKey, AutoIncrement]
        public int Id_Devolucion { get; set; }
        
        [NotNull]
        public int Id_Venta { get; set; }
        
        [NotNull]
        public int Id_Producto { get; set; }
        
        [NotNull]
        public int Cantidad { get; set; }
        
        [NotNull]
        public decimal MontoDevuelto { get; set; }
        
        [NotNull]
        public string Motivo { get; set; } = string.Empty;
        
        [NotNull]
        public int Id_Usuario { get; set; }
        
        [NotNull]
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        public string Observaciones { get; set; } = string.Empty;
    }
}
