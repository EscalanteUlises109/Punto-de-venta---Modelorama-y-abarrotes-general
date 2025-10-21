using IntegraTech_POS.Models;

namespace IntegraTech_POS.Services;

public interface IPromocionService
{
    Task<List<Promocion>> GetPromocionesAsync(bool soloActivas = true);
    Task<(decimal descuento, string nota)> CalcularDescuentoAsync(List<DetalleVenta> carrito);
    Task<int> GuardarPromocionAsync(Promocion promo, List<int> productoIds);
    Task<int> EliminarPromocionAsync(int promoId);
    Task<List<int>> GetProductoIdsAsync(int promoId);
}
