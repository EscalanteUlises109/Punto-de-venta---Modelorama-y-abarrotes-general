using System;

namespace IntegraTech_POS.Services
{
    public class EventService
    {
        public event Action? VentaRealizada;

        public void NotificarVentaRealizada()
        {
            VentaRealizada?.Invoke();
        }
    }
}