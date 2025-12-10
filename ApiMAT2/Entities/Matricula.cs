namespace ApiMAT2.Entities
{
    public class Matricula
    {
        public int ID_Matricula { get; set; }
        public int ID_Estudiante { get; set; }
        public int ID_Curso { get; set; }
        public int ID_Grupo { get; set; }
        public DateTime Fecha_Matricula { get; set; }

    }

    public class MatriculaEstudianteDto
    {
        public string Carrera_Estudiante { get; set; } = string.Empty;
        public DateTime Fecha_Matricula { get; set; }
        public string Nombre_Grupo { get; set; } = string.Empty;
        public string Nombre_Curso { get; set; } = string.Empty;
        public string Codigo_Curso { get; set; } = string.Empty;
        public string? Periodo_Actual { get; set; }
    }
}
