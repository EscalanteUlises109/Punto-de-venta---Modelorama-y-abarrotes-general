using Microsoft.Extensions.Logging;
using IntegraTech_POS.Services;
using IntegraTech_POS.Models;
using IntegraTech_POS.Validators;
using FluentValidation;
using Serilog;
using Serilog.Events;

namespace IntegraTech_POS
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            
            var logPath = Path.Combine(FileSystem.AppDataDirectory, "logs", "integratech-pos-.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            
            builder.Logging.AddSerilog();

            
            builder.Services.AddSingleton<DatabaseService>();

            
            builder.Services.AddScoped<IValidator<Producto>, ProductoValidator>();
            builder.Services.AddScoped<IValidator<Venta>, VentaValidator>();
            builder.Services.AddScoped<IValidator<Usuario>, UsuarioValidator>();

            
            builder.Services.AddScoped<IProductoService, ProductoService>();
            builder.Services.AddScoped<IVentaService, VentaService>();
            builder.Services.AddScoped<IImagenService, ImagenService>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();
            
            builder.Services.AddSingleton<ReportePDFService>();
            builder.Services.AddSingleton<EmailService>();
            builder.Services.AddSingleton<ReporteAutomaticoService>();
            builder.Services.AddSingleton<IPromocionService, PromocionService>();
            builder.Services.AddSingleton<AuthService>(); 
            builder.Services.AddSingleton<EventService>();
            builder.Services.AddSingleton<BarcodeScannerService>();
            
            builder.Services.AddScoped<ReceiptPrinterService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            Log.Information("🚀 IntegraTech-POS iniciando...");
            Log.Information("📁 Logs guardados en: {LogPath}", logPath);

            return builder.Build();
        }
    }
}

