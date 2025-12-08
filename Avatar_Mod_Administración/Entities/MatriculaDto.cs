

namespace Avatar_Mod_Administración.Entities
{
    public class MatriculaDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public string NombreGrupo { get; set; } = string.Empty;
        public DateTime Fecha_Matricula { get; set; }
    }
}
