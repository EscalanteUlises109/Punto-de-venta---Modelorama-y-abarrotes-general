using System;
using SQLite;

namespace IntegraTech_POS.Models
{
    [Table("Productos")]
    public class Producto
    {
        [PrimaryKey, AutoIncrement]
        public int Id_Producto { get; set; }
        
        [NotNull]
        public string Nombre_Producto { get; set; } = string.Empty;
        
        [NotNull]
        public decimal Precio_Venta { get; set; }
        
        public decimal Precio_Compra { get; set; }
        
        public string Distribuidor { get; set; } = string.Empty;
        
        public string Categoria { get; set; } = string.Empty;
        
        [NotNull]
        public int Cantidad { get; set; }
        
        public string ImagenPath { get; set; } = string.Empty;
        
        [Unique]
        public string Codigo_Barras { get; set; } = string.Empty;
        
        [NotNull]
        public DateTime Fecha_Registro { get; set; } = DateTime.Now;
        
        public DateTime? Fecha_Vencimiento { get; set; }
        
        public string Unidad_Medida { get; set; } = string.Empty;
        
        public int Stock_Minimo { get; set; } = 5;
    }
}
