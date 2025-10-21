using IntegraTech_POS.Models;

namespace IntegraTech_POS.Services
{
    public interface IVentaService
    {
        Task<List<Venta>> GetVentasAsync();
        Task<List<Venta>> GetVentasPorFechaAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<bool> GuardarVentaAsync(Venta venta, List<DetalleVenta> detalles);
        Task<List<DetalleVenta>> GetDetalleVentaAsync(int ventaId);
        Task<decimal> GetTotalVentasDiaAsync(DateTime fecha);
        Task<decimal> GetTotalVentasMesAsync(DateTime fecha);
    }
}