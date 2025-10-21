using SQLite;

namespace IntegraTech_POS.Models;

[Table("PromocionProductos")]
public class PromocionProducto
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PromocionId { get; set; }

    [Indexed]
    public int ProductoId { get; set; }
}
