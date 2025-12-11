namespace ACD2.Entities
{
    public class Carrera
    {
        public int IdCarrera { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdInstitucion { get; set; }
        public int IdDirector { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
