using IntegraTech_POS.Models;
using Microsoft.Extensions.Logging;

namespace IntegraTech_POS.Services;

public class PromocionService : IPromocionService
{
    private readonly DatabaseService _db;
    private readonly ILogger<PromocionService> _logger;

    public PromocionService(DatabaseService db, ILogger<PromocionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Task<List<Promocion>> GetPromocionesAsync(bool soloActivas = true)
        => _db.GetPromocionesAsync(soloActivas);

    public Task<List<int>> GetProductoIdsAsync(int promoId)
        => _db.GetProductoIdsDePromocionAsync(promoId);

    public Task<int> GuardarPromocionAsync(Promocion promo, List<int> productoIds)
        => _db.SavePromocionAsync(promo, productoIds);

    public Task<int> EliminarPromocionAsync(int promoId)
        => _db.DeletePromocionAsync(promoId);

    
    
    
    
    
    
    public async Task<(decimal descuento, string nota)> CalcularDescuentoAsync(List<DetalleVenta> carrito)
    {
        try
        {
            var promos = await _db.GetPromocionesAsync(soloActivas: true);
            decimal descuentoTotal = 0m;
            var notas = new List<string>();

            foreach (var promo in promos)
            {
                if (!string.Equals(promo.Tipo, "BundleFijo", StringComparison.OrdinalIgnoreCase))
                    continue;

                var productoIds = await _db.GetProductoIdsDePromocionAsync(promo.Id);
                if (productoIds.Count == 0) continue;

                
                var preciosElegibles = new List<decimal>();
                foreach (var det in carrito)
                {
                    if (productoIds.Contains(det.Id_Producto))
                    {
                        for (int i = 0; i < det.Cantidad; i++)
                            preciosElegibles.Add(det.Precio_Unitario);
                    }
                }

                if (preciosElegibles.Count < promo.CantidadGrupo || promo.CantidadGrupo <= 0)
                    continue;

                
                preciosElegibles.Sort((a,b) => b.CompareTo(a));
                int grupos = preciosElegibles.Count / promo.CantidadGrupo;
                if (grupos <= 0) continue;

                for (int g = 0; g < grupos; g++)
                {
                    var slice = preciosElegibles.Skip(g * promo.CantidadGrupo).Take(promo.CantidadGrupo).ToList();
                    var precioNormalGrupo = slice.Sum();
                    var descuentoGrupo = Math.Max(0, precioNormalGrupo - promo.PrecioGrupo);
                    if (descuentoGrupo > 0)
                    {
                        descuentoTotal += descuentoGrupo;
                    }
                }

                if (descuentoTotal > 0 && grupos > 0)
                {
                    notas.Add($"Promo '{promo.Nombre}': {grupos}x {promo.CantidadGrupo} por ${promo.PrecioGrupo:N2}");
                }
            }

            var nota = string.Join(" | ", notas);
            return (Math.Round(descuentoTotal, 2), nota);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando descuento por promociones");
            return (0m, string.Empty);
        }
    }
}

