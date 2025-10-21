using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraTech_POS.Models;

public class GananciaProducto
{
    public string NombreProducto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Distribuidor { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal GananciaPorUnidad { get; set; }
    public int CantidadVendida { get; set; }
    public decimal GananciaTotal { get; set; }
    public decimal PorcentajeMargen => PrecioVenta > 0 ? (GananciaPorUnidad / PrecioVenta * 100) : 0;
}