using SQLite;

namespace IntegraTech_POS.Models;

[Table("Promociones")]
public class Promocion
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Nombre { get; set; } = string.Empty;

    
    [NotNull]
    public string Tipo { get; set; } = "BundleFijo";

    
    [NotNull]
    public int CantidadGrupo { get; set; }

    
    [NotNull]
    public decimal PrecioGrupo { get; set; }

    
    public bool Activa { get; set; } = true;
}

