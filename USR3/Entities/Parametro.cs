namespace USR3.Entities
{
    public class Parametro
    {
        public string IdParametro { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
