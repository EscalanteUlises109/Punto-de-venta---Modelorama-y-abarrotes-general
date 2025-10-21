using IntegraTech_POS.Models;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace IntegraTech_POS.Services
{
    public class VentaService : IVentaService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<VentaService> _logger;
        private readonly IValidator<Venta> _validator;
        private readonly AuthService _authService;

        public VentaService(
            DatabaseService databaseService,
            ILogger<VentaService> logger,
            IValidator<Venta> validator,
            AuthService authService)
        {
            _databaseService = databaseService;
            _logger = logger;
            _validator = validator;
            _authService = authService;
        }

        public async Task<List<Venta>> GetVentasAsync()
        {
            try
            {
                return await _databaseService.GetVentasAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ventas: {ex.Message}");
                return new List<Venta>();
            }
        }

        public async Task<List<Venta>> GetVentasPorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                return await _databaseService.GetVentasPorFechaAsync(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo ventas por fecha: {ex.Message}");
                return new List<Venta>();
            }
        }

        public async Task<bool> GuardarVentaAsync(Venta venta, List<DetalleVenta> detalles)
        {
            try
            {
                
                var validationResult = await _validator.ValidateAsync(venta);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("ValidaciÃ³n fallida al guardar venta: {Errors}", 
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    return false;
                }

                
                if (detalles == null || !detalles.Any())
                {
                    _logger.LogWarning("Intento de guardar venta sin detalles");
                    return false;
                }

                
                var usuarioId = _authService.UsuarioActual?.Id_Usuario ?? 0;
                var ok = await _databaseService.GuardarVentaConTransaccionAsync(venta, detalles, usuarioId);

                if (ok)
                {
                    _logger.LogInformation("Venta guardada exitosamente con transacciÃ³n. VentaId: {VentaId}, Total: ${Total:N2}", 
                        venta.Id_Venta, venta.Total);
                }

                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando venta");
                return false;
            }
        }

        public async Task<List<DetalleVenta>> GetDetalleVentaAsync(int ventaId)
        {
            try
            {
                return await _databaseService.GetDetalleVentaAsync(ventaId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo detalle venta: {ex.Message}");
                return new List<DetalleVenta>();
            }
        }

        public async Task<decimal> GetTotalVentasDiaAsync(DateTime fecha)
        {
            try
            {
                return await _databaseService.GetVentasDelDiaAsync(fecha);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo total ventas dÃ­a: {ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> GetTotalVentasMesAsync(DateTime fecha)
        {
            try
            {
                return await _databaseService.GetVentasTotalMesAsync(fecha);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo total ventas mes: {ex.Message}");
                return 0;
            }
        }
    }
}
