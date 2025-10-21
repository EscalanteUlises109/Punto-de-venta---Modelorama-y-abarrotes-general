using IntegraTech_POS.Models;

namespace IntegraTech_POS.Services
{
    public class ReporteAutomaticoService
    {
        private readonly DatabaseService _databaseService;
    private readonly ReportePDFService _reportePdfService;
    private readonly EmailService _emailService;
        private System.Threading.Timer? _timer;
        private bool _isRunning = false;

        public ReporteAutomaticoService(DatabaseService databaseService, ReportePDFService reportePdfService, EmailService emailService)
        {
            _databaseService = databaseService;
            _reportePdfService = reportePdfService;
            _emailService = emailService;
        }

        public async Task IniciarServicioAsync()
        {
            try
            {
                
                var habilitado = await _databaseService.ObtenerConfiguracionBoolAsync("ReporteAutomatico", false);
                
                if (habilitado)
                {
                    var horaString = await _databaseService.ObtenerConfiguracionAsync("HoraReporteAutomatico");
                    
                    if (!string.IsNullOrEmpty(horaString) && TimeSpan.TryParse(horaString, out var horaConfigurada))
                    {
                        await IniciarTemporizador(horaConfigurada);
                        Console.WriteLine($"âœ… Servicio de reportes automÃ¡ticos iniciado para las {horaConfigurada:hh\\:mm}");
                    }
                    else
                    {
                        Console.WriteLine("âš ï¸ No se pudo leer la hora configurada para el reporte automÃ¡tico");
                    }
                }
                else
                {
                    Console.WriteLine("â„¹ï¸ Reporte automÃ¡tico deshabilitado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error iniciando servicio de reportes automÃ¡ticos: {ex.Message}");
            }
        }

        public async Task IniciarTemporizador(TimeSpan horaObjetivo)
        {
            await Task.Run(() =>
            {
                
                _timer?.Dispose();
                
                _isRunning = true;
                
                
                var ahora = DateTime.Now;
                var horaHoy = DateTime.Today.Add(horaObjetivo);
                
                
                var proximaEjecucion = horaHoy > ahora ? horaHoy : horaHoy.AddDays(1);
                var tiempoHastaEjecucion = proximaEjecucion - ahora;
                
                Console.WriteLine($"â° PrÃ³ximo reporte automÃ¡tico programado para: {proximaEjecucion:dd/MM/yyyy HH:mm:ss}");
                Console.WriteLine($"â±ï¸ Tiempo restante: {tiempoHastaEjecucion.TotalHours:F2} horas");
                
                
                _timer = new System.Threading.Timer(
                    callback: async _ => await GenerarReporteAutomaticoAsync(),
                    state: null,
                    dueTime: tiempoHastaEjecucion,
                    period: TimeSpan.FromDays(1) 
                );
            });
        }

        public void DetenerTemporizador()
        {
            _timer?.Dispose();
            _timer = null;
            _isRunning = false;
            Console.WriteLine("â¸ï¸ Servicio de reportes automÃ¡ticos detenido");
        }

        private async Task GenerarReporteAutomaticoAsync()
        {
            try
            {
                Console.WriteLine($"ðŸ¤– Generando reporte automÃ¡tico - {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                
                await _databaseService.InitializeAsync();
                
                
                var ventasHoy = await _databaseService.GetVentasAsync();
                ventasHoy = ventasHoy?.Where(v => v.Fecha_Venta.Date == DateTime.Today).ToList();
                
                if (ventasHoy == null || !ventasHoy.Any())
                {
                    Console.WriteLine("â„¹ï¸ No hay ventas para generar reporte automÃ¡tico");
                    return;
                }
                
                
                var pdfPath = await _reportePdfService.GenerarReporteVentasPDFAsync(ventasHoy, DateTime.Today, _databaseService);
                var csvPath = await GenerarCSVAsync(ventasHoy, DateTime.Today);
                
                if (!string.IsNullOrEmpty(pdfPath))
                {
                    Console.WriteLine($"âœ… Reporte PDF automÃ¡tico generado: {Path.GetFileName(pdfPath)}");
                }
                
                if (!string.IsNullOrEmpty(csvPath))
                {
                    Console.WriteLine($"âœ… Reporte CSV automÃ¡tico generado: {Path.GetFileName(csvPath)}");
                }
                
                
                var emailTo = await _databaseService.ObtenerConfiguracionAsync("REPORT_EMAIL_TO");
                var emailCc = await _databaseService.ObtenerConfiguracionAsync("REPORT_EMAIL_CC");
                var emailBcc = await _databaseService.ObtenerConfiguracionAsync("REPORT_EMAIL_BCC");
                if (!string.IsNullOrWhiteSpace(emailTo))
                {
                    var adjuntos = new List<string>();
                    if (!string.IsNullOrEmpty(pdfPath) && File.Exists(pdfPath)) adjuntos.Add(pdfPath);
                    if (!string.IsNullOrEmpty(csvPath) && File.Exists(csvPath)) adjuntos.Add(csvPath);
                    if (adjuntos.Any())
                    {
                        var ok = await _emailService.EnviarAsync(emailTo, $"Reportes automÃ¡ticos {DateTime.Today:dd/MM/yyyy}", "Se adjuntan los reportes automÃ¡ticos generados.", adjuntos, emailCc, emailBcc);
                        Console.WriteLine(ok ? "ðŸ“§ Reportes enviados por correo" : "âš ï¸ No se pudo enviar reportes por correo (ver SMTP)");
                    }
                }

                
                await _databaseService.RegistrarAuditoriaAsync(
                    usuarioId: 0,
                    nombreUsuario: "Sistema",
                    accion: "Reporte AutomÃ¡tico Generado",
                    detalles: $"Reportes generados automÃ¡ticamente. Total ventas: {ventasHoy.Count}, Total recaudado: ${ventasHoy.Sum(v => v.Total):N2}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error en reporte automÃ¡tico: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }

        private async Task<string> GenerarCSVAsync(List<Venta> ventas, DateTime fecha)
        {
            try
            {
                var fileName = $"Reporte_Automatico_{fecha:yyyy-MM-dd}.csv";
                var directorio = Path.Combine(FileSystem.AppDataDirectory, "Reportes");
                
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }
                
                var filePath = Path.Combine(directorio, fileName);
                
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("# Venta,Fecha,Hora,Cliente,MÃ©todo Pago,Producto,CategorÃ­a,Proveedor,Cantidad,Precio Unit.,Subtotal,Total Venta,Descuento,Notas");
                
                foreach (var venta in ventas.OrderBy(v => v.Fecha_Venta))
                {
                    var detalles = await _databaseService.GetDetallesVentaConProductosAsync(venta.Id_Venta);
                    
                    if (detalles.Any())
                    {
                        foreach (var detalle in detalles)
                        {
                            csv.AppendLine($"{venta.Id_Venta}," +
                                         $"{venta.Fecha_Venta:dd/MM/yyyy}," +
                                         $"{venta.Fecha_Venta:HH:mm:ss}," +
                                         $"\"{(string.IsNullOrEmpty(venta.Cliente) ? "Cliente general" : venta.Cliente)}\"," +
                                         $"\"{(string.IsNullOrEmpty(venta.Metodo_Pago) ? "Efectivo" : venta.Metodo_Pago)}\"," +
                                         $"\"{detalle.Producto?.Nombre_Producto ?? "N/A"}\"," +
                                         $"\"{(string.IsNullOrEmpty(detalle.Producto?.Categoria) ? "Sin categorÃ­a" : detalle.Producto.Categoria)}\"," +
                                         $"\"{(string.IsNullOrEmpty(detalle.Producto?.Distribuidor) ? "Sin proveedor" : detalle.Producto.Distribuidor)}\"," +
                                         $"{detalle.Cantidad}," +
                                         $"{detalle.Precio_Unitario:N2}," +
                                         $"{detalle.Subtotal:N2}," +
                                         $"{venta.Total:N2}," +
                                         $"{venta.Descuento:N2}," +
                                         $"\"{(string.IsNullOrEmpty(venta.Notas) ? "" : venta.Notas)}\"");
                        }
                    }
                }
                
                await File.WriteAllTextAsync(filePath, csv.ToString());
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error generando CSV automÃ¡tico: {ex.Message}");
                return string.Empty;
            }
        }

        public bool EstaActivo() => _isRunning;

        public async Task<TimeSpan?> ObtenerHoraConfiguradaAsync()
        {
            var horaString = await _databaseService.ObtenerConfiguracionAsync("HoraReporteAutomatico");
            
            if (!string.IsNullOrEmpty(horaString) && TimeSpan.TryParse(horaString, out var hora))
            {
                return hora;
            }
            
            return null;
        }

        public async Task<DateTime?> ObtenerProximaEjecucionAsync()
        {
            if (!_isRunning) return null;
            
            var horaString = await _databaseService.ObtenerConfiguracionAsync("HoraReporteAutomatico");
            
            if (!string.IsNullOrEmpty(horaString) && TimeSpan.TryParse(horaString, out var horaObjetivo))
            {
                var ahora = DateTime.Now;
                var horaHoy = DateTime.Today.Add(horaObjetivo);
                return horaHoy > ahora ? horaHoy : horaHoy.AddDays(1);
            }
            
            return null;
        }
    }
}

