using System;
using SQLite;

namespace IntegraTech_POS.Models
{
    [Table("Cajas")]
    public class Caja
    {
        [PrimaryKey, AutoIncrement]
        public int Id_Caja { get; set; }
        
        [NotNull]
        public string Nombre { get; set; } = string.Empty;
        
        [NotNull]
        public int Id_Usuario { get; set; }
        
        [NotNull]
        public decimal MontoInicial { get; set; }
        
        [NotNull]
        public decimal MontoFinal { get; set; }
        
        [NotNull]
        public DateTime FechaApertura { get; set; } = DateTime.Now;
        
        public DateTime? FechaCierre { get; set; }
        
        [NotNull]
        public bool Abierta { get; set; } = true;
        
        public string Notas { get; set; } = string.Empty;
    }
}
