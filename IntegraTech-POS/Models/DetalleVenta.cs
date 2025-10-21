using System;
using SQLite;

namespace IntegraTech_POS.Models;

[Table("DetalleVenta")]
public class DetalleVenta
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public int VentaId { get; set; }
    
    [Indexed]  
    public int Id_Producto { get; set; }
    
    
    [Ignore]
    public int ProductoId 
    { 
        get => Id_Producto; 
        set => Id_Producto = value; 
    }
    
    public int Cantidad { get; set; }
    
    public decimal Precio_Unitario { get; set; }
    
    
    [Ignore]
    public decimal PrecioUnitario 
    { 
        get => Precio_Unitario; 
        set => Precio_Unitario = value; 
    }
    
    public decimal Subtotal { get; set; }
    
    
    [Ignore]
    public Producto? Producto { get; set; }
    
    [Ignore]
    public string? NombreProducto { get; set; }
}
