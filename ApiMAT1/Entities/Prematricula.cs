namespace ApiMAT1.Entities
{
    public class Prematricula
    {
        public int ID_Prematricula { get; set; }
        public int ID_Estudiante { get; set; }
        public int ID_Carrera { get; set; }
        public int ID_Curso { get; set; }
        public string? Observaciones { get; set; }
        public int ID_Periodo { get; set; }


    }

    public class PrematriculaDetallada
    {
        // Campos del Estudiante
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre_Completo { get; set; } = string.Empty;
        public string Carrera_Estudiante { get; set; } = string.Empty; // Código de carrera del estudiante

        // Campos de Prematricula
        public string Observaciones { get; set; } = string.Empty;

        // Campos de Carrera (del curso prematriculado)
        public string Nombre_Carrera { get; set; } = string.Empty;

        // Campos de Curso
        public string Nombre_Curso { get; set; } = string.Empty;
        public string Codigo_Curso { get; set; } = string.Empty;

        // Campos de Periodo
        public int Año { get; set; }
        public int Numero_Periodo { get; set; }
        public DateTime Fecha_Inicio { get; set; }
        public DateTime Fecha_Fin { get; set; }
    }
}
