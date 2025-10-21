using System;
using SQLite;

namespace IntegraTech_POS.Models
{
    [Table("Ventas")]
    public class Venta
    {
        [PrimaryKey, AutoIncrement]
        public int Id_Venta { get; set; }
        
    [NotNull, Indexed]
    public DateTime Fecha_Venta { get; set; } = DateTime.Now;
        
        [NotNull]
        public decimal Total { get; set; }
        
        public string Cliente { get; set; } = string.Empty;
        
        public string Metodo_Pago { get; set; } = string.Empty;
        
        public decimal Descuento { get; set; }
        
        public decimal Impuesto { get; set; }
        
        public string Notas { get; set; } = string.Empty;

        
        public decimal PagoRecibido { get; set; } = 0m;
        public decimal Cambio { get; set; } = 0m;
    }
}
