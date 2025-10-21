using SQLite;

namespace IntegraTech_POS.Models
{
    [Table("ConfiguracionSistema")]
    public class ConfiguracionSistema
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Clave { get; set; } = string.Empty;

        public string Valor { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaModificacion { get; set; } = DateTime.Now;
    }
}
