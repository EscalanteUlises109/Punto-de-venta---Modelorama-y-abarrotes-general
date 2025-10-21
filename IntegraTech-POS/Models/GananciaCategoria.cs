using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegraTech_POS.Models
{
    public class GananciaCategoria
    {
        public string Categoria { get; set; } = string.Empty;
        public int CantidadProductos { get; set; }
        public int TotalVendido { get; set; }
        public decimal VentaTotal { get; set; }
        public decimal GananciaTotal { get; set; }
        public decimal PorcentajeMargen => VentaTotal > 0 ? (GananciaTotal / VentaTotal * 100) : 0;
    }
}
