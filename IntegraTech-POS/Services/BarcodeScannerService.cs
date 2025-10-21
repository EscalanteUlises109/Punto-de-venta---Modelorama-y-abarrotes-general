using Microsoft.JSInterop;

namespace IntegraTech_POS.Services
{
    public class BarcodeScannerService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<BarcodeScannerService>? _objRef;
        private string _buffer = "";
        private DateTime _lastKeyTime = DateTime.Now;
    private const int SCAN_THRESHOLD_MS = 80; 
        private bool _initialized = false;
        
        public event Action<string>? OnBarcodeScanned;

        public BarcodeScannerService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
            {
                Console.WriteLine("ðŸ“· EscÃ¡ner ya inicializado, reutilizando instancia");
                return;
            }

            try
            {
                _objRef = DotNetObjectReference.Create(this);
                await _jsRuntime.InvokeVoidAsync("barcodeScanner.initialize", _objRef);
                _initialized = true;
                Console.WriteLine("ðŸ“· Servicio de escÃ¡ner de cÃ³digos de barras inicializado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"âŒ Error inicializando escÃ¡ner: {ex.Message}");
            }
        }

        [JSInvokable]
        public void OnKeyPress(string key, long timestamp)
        {
            var now = DateTime.Now;
            var timeDiff = (now - _lastKeyTime).TotalMilliseconds;

            
            if (timeDiff > 100)
            {
                _buffer = "";
            }

            _lastKeyTime = now;

            
            if ((key == "Enter" || key == "Tab") && !string.IsNullOrEmpty(_buffer))
            {
                var barcode = _buffer.Trim();
                Console.WriteLine($"ðŸ“· CÃ³digo de barras escaneado: {barcode}");
                OnBarcodeScanned?.Invoke(barcode);
                _buffer = "";
            }
            else if (key.Length == 1) 
            {
                _buffer += key;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_objRef != null)
            {
                await _jsRuntime.InvokeVoidAsync("barcodeScanner.dispose");
                _objRef.Dispose();
            }
        }
    }
}

