using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using IntegraTech_POS.Models;
using SkiaSharp;

namespace IntegraTech_POS.Services
{
    public class ReceiptPrinterService
    {
        private readonly DatabaseService _db;
        private readonly ILogger<ReceiptPrinterService> _logger;
        private readonly IImagenService _imagenes;

        public ReceiptPrinterService(DatabaseService db, ILogger<ReceiptPrinterService> logger, IImagenService imagenes)
        {
            _db = db;
            _logger = logger;
            _imagenes = imagenes;
        }

        public async Task<bool> PrintTicketAsync(Venta venta, List<DetalleVenta> detalles)
        {
            try
            {
                
                var ip = await _db.ObtenerConfiguracionAsync("PRINTER_IP");
                var portStr = await _db.ObtenerConfiguracionAsync("PRINTER_PORT");
                var printerName = await _db.ObtenerConfiguracionAsync("PRINTER_NAME");
                var comPort = await _db.ObtenerConfiguracionAsync("PRINTER_COM_PORT");
                var baudStr = await _db.ObtenerConfiguracionAsync("PRINTER_BAUD");
                var tienda = await _db.ObtenerConfiguracionAsync("TIENDA_NOMBRE") ?? "IntegraTech POS";
                var direccion = await _db.ObtenerConfiguracionAsync("TIENDA_DIRECCION") ?? string.Empty;
                var telefono = await _db.ObtenerConfiguracionAsync("TIENDA_TELEFONO") ?? string.Empty;
                var logoPath = await _db.ObtenerConfiguracionAsync("TIENDA_LOGO_PATH") ?? string.Empty;
                var extra1 = await _db.ObtenerConfiguracionAsync("TIENDA_EXTRA1") ?? string.Empty;
                var extra2 = await _db.ObtenerConfiguracionAsync("TIENDA_EXTRA2") ?? string.Empty;
                var extra3 = await _db.ObtenerConfiguracionAsync("TIENDA_EXTRA3") ?? string.Empty;
                var widthDotsStr = await _db.ObtenerConfiguracionAsync("PRINTER_WIDTH_DOTS");
                int maxWidthDots = 384; 
                if (!string.IsNullOrWhiteSpace(widthDotsStr) && int.TryParse(widthDotsStr, out var w)) maxWidthDots = Math.Max(64, w);

                
                var buffer = new List<byte>(1024);
                void Write(byte[] bytes) => buffer.AddRange(bytes);
                void Txt(string text) => Write(Encoding.UTF8.GetBytes(text));

                byte[] INIT = new byte[] { 0x1B, 0x40 };
                byte[] CENTER = new byte[] { 0x1B, 0x61, 0x01 };
                byte[] LEFT = new byte[] { 0x1B, 0x61, 0x00 };
                byte[] RIGHT = new byte[] { 0x1B, 0x61, 0x02 };
                byte[] BOLD_ON = new byte[] { 0x1B, 0x45, 0x01 };
                byte[] BOLD_OFF = new byte[] { 0x1B, 0x45, 0x00 };
                byte[] DOUBLE_ON = new byte[] { 0x1D, 0x21, 0x11 }; 
                byte[] DOUBLE_OFF = new byte[] { 0x1D, 0x21, 0x00 };
                byte[] CUT = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

                Write(INIT);
                Write(CENTER);
                
                if (!string.IsNullOrWhiteSpace(logoPath))
                {
                    var cmd = await TryBuildLogoEscPosAsync(logoPath, maxWidthDots);
                    if (cmd != null)
                    {
                        Write(cmd);
                        Txt("\n");
                    }
                }
                Write(DOUBLE_ON);
                Txt($"{tienda}\n");
                Write(DOUBLE_OFF);
                if (!string.IsNullOrWhiteSpace(direccion)) Txt($"{direccion}\n");
                if (!string.IsNullOrWhiteSpace(telefono)) Txt($"Tel: {telefono}\n");
                if (!string.IsNullOrWhiteSpace(extra1)) Txt($"{extra1}\n");
                if (!string.IsNullOrWhiteSpace(extra2)) Txt($"{extra2}\n");
                if (!string.IsNullOrWhiteSpace(extra3)) Txt($"{extra3}\n");
                Txt("------------------------------------------\n");

                
                Txt($"Venta #{venta.Id_Venta}\n");
                Txt($"Fecha: {venta.Fecha_Venta:dd/MM/yyyy HH:mm:ss}\n");
                Txt($"Pago: {(string.IsNullOrWhiteSpace(venta.Metodo_Pago) ? "Efectivo" : venta.Metodo_Pago)}\n");
                if (!string.IsNullOrWhiteSpace(venta.Cliente)) Txt($"Cliente: {venta.Cliente}\n");
                Txt("------------------------------------------\n");

                
                Write(LEFT);
                foreach (var d in detalles)
                {
                    var nombre = d.Producto?.Nombre_Producto ?? $"Prod #{d.Id_Producto}";
                    var qty = d.Cantidad;
                    var precio = d.Precio_Unitario;
                    var subtotal = d.Subtotal;
                    
                    Txt(Trunc(nombre, 42) + "\n");
                    
                    var line = $"{qty} x {precio:N2}";
                    var right = $"${subtotal:N2}";
                    Txt(LineJustify(line, right, 42) + "\n");
                }

                Txt("------------------------------------------\n");
                var subTotal = detalles.Sum(x => x.Subtotal);
                var descuento = venta.Descuento;
                var iva = venta.Impuesto;
                var total = venta.Total;
                Txt(LineJustify("SUBTOTAL:", $"${subTotal:N2}", 42) + "\n");
                if (descuento > 0)
                    Txt(LineJustify("DESCUENTO:", $"-${descuento:N2}", 42) + "\n");
                
                var ivaActivoCfg = await _db.ObtenerConfiguracionAsync("IVA_ACTIVO");
                var ivaActivo = string.Equals(ivaActivoCfg, "true", StringComparison.OrdinalIgnoreCase);
                if (ivaActivo && iva > 0)
                    Txt(LineJustify("IVA:", $"${iva:N2}", 42) + "\n");
                Txt(LineJustify("TOTAL:", $"${total:N2}", 42) + "\n");

                
                if (!string.IsNullOrWhiteSpace(venta.Notas))
                {
                    Txt("------------------------------------------\n");
                    Txt(Trunc($"Promo: {venta.Notas}", 42) + "\n");
                }

                
                if (venta.Metodo_Pago?.Equals("Efectivo", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (venta.PagoRecibido > 0)
                        Txt(LineJustify("PAGO:", $"${venta.PagoRecibido:N2}", 42) + "\n");
                    if (venta.Cambio > 0)
                        Txt(LineJustify("CAMBIO:", $"${venta.Cambio:N2}", 42) + "\n");
                }

                Txt("\nGracias por su compra\n\n\n");
                Write(CUT);
                
#if WINDOWS
                if (!string.IsNullOrWhiteSpace(printerName))
                {
                    var okUsb = Platforms.Windows.Printing.RawPrinterHelper.SendBytesToPrinter(printerName, buffer.ToArray());
                    if (okUsb)
                    {
                        _logger.LogInformation("Ticket impreso correctamente en impresora USB '{Printer}'", printerName);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Fallo impresiÃ³n USB en '{Printer}', intentando TCP si estÃ¡ configurado...", printerName);
                    }
                }
#endif

                
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    int port = 9100;
                    if (!string.IsNullOrWhiteSpace(portStr) && int.TryParse(portStr, out var parsed))
                        port = parsed;

                    using var client = new TcpClient();
                    await client.ConnectAsync(ip, port);
                    using var stream = client.GetStream();
                    var bytes = buffer.ToArray();
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    await stream.FlushAsync();
                    _logger.LogInformation("Ticket impreso correctamente en {Ip}:{Port}", ip, port);
                    return true;
                }

                
                if (!string.IsNullOrWhiteSpace(comPort))
                {
#if WINDOWS
                    try
                    {
                        int baud = 9600;
                        if (!string.IsNullOrWhiteSpace(baudStr) && int.TryParse(baudStr, out var b)) baud = b;
                        using var serial = new System.IO.Ports.SerialPort(comPort, baud);
                        serial.Parity = System.IO.Ports.Parity.None;
                        serial.DataBits = 8;
                        serial.StopBits = System.IO.Ports.StopBits.One;
                        serial.Handshake = System.IO.Ports.Handshake.None;
                        serial.Open();
                        var bytes = buffer.ToArray();
                        serial.Write(bytes, 0, bytes.Length);
                        serial.BaseStream.Flush();
                        serial.Close();
                        _logger.LogInformation("Ticket impreso correctamente por COM {ComPort} a {Baud} baudios", comPort, baud);
                        return true;
                    }
                    catch (Exception sex)
                    {
                        _logger.LogError(sex, "Error imprimiendo por COM {ComPort}", comPort);
                    }
#else
                    _logger.LogWarning("ImpresiÃ³n por COM no soportada en esta plataforma.");
#endif
                }

                _logger.LogWarning("No hay configuraciÃ³n de impresora vÃ¡lida (PRINTER_NAME, PRINTER_IP o PRINTER_COM_PORT). Omitiendo impresiÃ³n.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error imprimiendo ticket");
                return false;
            }
        }

#if WINDOWS
    private async Task<byte[]?> TryBuildLogoEscPosAsync(string logoNombreArchivo, int maxWidthDots)
        {
            try
            {
                var ruta = _imagenes.ObtenerRutaCompleta(logoNombreArchivo);
                if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta)) return null;

                using var bmp = new System.Drawing.Bitmap(ruta);
                
                int targetW = Math.Min(maxWidthDots, bmp.Width);
                int targetH = (int)Math.Round(bmp.Height * (targetW / (double)bmp.Width));
                using var scaled = new System.Drawing.Bitmap(bmp, new System.Drawing.Size(targetW, targetH));

                
                int width = scaled.Width;
                int height = scaled.Height;
                int bytesPerRow = (width + 7) / 8;
                var data = new byte[bytesPerRow * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var color = scaled.GetPixel(x, y);
                        
                        int lum = (color.R * 299 + color.G * 587 + color.B * 114) / 1000;
                        bool isBlack = lum < 160; 
                        if (isBlack)
                        {
                            int index = y * bytesPerRow + (x >> 3);
                            data[index] |= (byte)(0x80 >> (x & 0x07));
                        }
                    }
                }

                
                byte m = 0x00; 
                byte[] header = new byte[] { 0x1D, 0x76, 0x30, m, (byte)(bytesPerRow & 0xFF), (byte)((bytesPerRow >> 8) & 0xFF), (byte)(height & 0xFF), (byte)((height >> 8) & 0xFF) };
                var output = new List<byte>(header.Length + data.Length + 8);
                
                output.AddRange(new byte[] { 0x1B, 0x61, 0x01 });
                output.AddRange(header);
                output.AddRange(data);
                return output.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo rasterizar el logo para ESC/POS");
                return null;
            }
        }
#else
    private async Task<byte[]?> TryBuildLogoEscPosAsync(string logoNombreArchivo, int maxWidthDots)
    {
        try
        {
            var ruta = _imagenes.ObtenerRutaCompleta(logoNombreArchivo);
            if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta)) return null;

            
            using var input = File.OpenRead(ruta);
            using var skData = SKData.Create(input);
            using var codec = SKCodec.Create(skData);
            if (codec == null) return null;

            var info = codec.Info;
            using var bitmap = SKBitmap.Decode(codec);
            if (bitmap == null) return null;

            
            int targetW = Math.Min(maxWidthDots, bitmap.Width);
            int targetH = (int)Math.Round(bitmap.Height * (targetW / (double)bitmap.Width));
            if (targetW <= 0 || targetH <= 0) return null;

            using var resized = new SKBitmap(targetW, targetH, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            using (var canvas = new SKCanvas(resized))
            {
                canvas.Clear(SKColors.White);
                var dest = new SKRect(0, 0, targetW, targetH);
                canvas.DrawBitmap(bitmap, dest, paint: new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.Medium });
                canvas.Flush();
            }

            
            int width = resized.Width;
            int height = resized.Height;
            int bytesPerRow = (width + 7) / 8;
            var data = new byte[bytesPerRow * height];

            
            
            const int threshold = 160;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var color = resized.GetPixel(x, y);
                    
                    int lum = (color.Red * 299 + color.Green * 587 + color.Blue * 114) / 1000;
                    bool isBlack = lum < threshold && color.Alpha > 10;
                    if (isBlack)
                    {
                        int index = y * bytesPerRow + (x >> 3);
                        data[index] |= (byte)(0x80 >> (x & 0x07));
                    }
                }
            }

            
            byte m = 0x00; 
            byte[] header = new byte[]
            {
                0x1D, 0x76, 0x30, m,
                (byte)(bytesPerRow & 0xFF), (byte)((bytesPerRow >> 8) & 0xFF),
                (byte)(height & 0xFF), (byte)((height >> 8) & 0xFF)
            };
            var output = new List<byte>(header.Length + data.Length + 8);
            
            output.AddRange(new byte[] { 0x1B, 0x61, 0x01 });
            output.AddRange(header);
            output.AddRange(data);
            return await Task.FromResult(output.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo rasterizar el logo (SkiaSharp) para ESC/POS");
            return null;
        }
    }
#endif

        private static string Trunc(string text, int max)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= max ? text : text.Substring(0, max);
        }

        private static string LineJustify(string left, string right, int width)
        {
            left = left ?? string.Empty;
            right = right ?? string.Empty;
            var space = width - left.Length - right.Length;
            if (space < 1) space = 1;
            return left + new string(' ', space) + right;
        }
    }
}

