namespace Avatar_Mod_Administración.Entities
{
    public class Institucion
    {
        public int IdInstitucion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; }
    }

    public class InstitucionCrearDto
    {
        public string Nombre { get; set; } = string.Empty;
    }
}