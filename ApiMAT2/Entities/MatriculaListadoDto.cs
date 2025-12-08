namespace ApiMAT2.Entities
{
    public class MatriculaListadoDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public string NombreGrupo { get; set; } = string.Empty;
        public DateTime Fecha_Matricula { get; set; }
    }
}
