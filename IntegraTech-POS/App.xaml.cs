using IntegraTech_POS.Services;

namespace IntegraTech_POS
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();

#if DEBUG
            
            Task.Run(async () => await ResetearAdminAlIniciar());
#endif

            
            Task.Run(async () => await IniciarReportesAutomaticos());
        }

        private async Task ResetearAdminAlIniciar()
        {
            try
            {
                await Task.Delay(1000); 
                
                var dbService = Handler.MauiContext?.Services.GetService<DatabaseService>();
                if (dbService != null)
                {
                    Console.WriteLine("========================================");
                    Console.WriteLine("🔧 INICIALIZANDO Y RESETEANDO ADMIN");
                    Console.WriteLine("========================================");
                    
                    
                    await dbService.InitializeAsync();
                    Console.WriteLine("✅ Base de datos inicializada");
                    
                    
                    await dbService.DiagnosticarUsuarioAdminAsync();
                    await dbService.ResetearPasswordAdminAsync();
                    await dbService.DiagnosticarUsuarioAdminAsync();
                    
                    Console.WriteLine("========================================");
                    Console.WriteLine("✅ Usuario admin listo para usar:");
                    Console.WriteLine("   Usuario: admin");
                    Console.WriteLine("   Contraseña: admin123");
                    Console.WriteLine("========================================");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reseteando admin: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }

        private async Task IniciarReportesAutomaticos()
        {
            try
            {
                await Task.Delay(1500); 
                var reporteAuto = Handler.MauiContext?.Services.GetService<ReporteAutomaticoService>();
                if (reporteAuto != null)
                {
                    await reporteAuto.IniciarServicioAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error iniciando reportes automáticos: {ex.Message}");
            }
        }
    }
}

