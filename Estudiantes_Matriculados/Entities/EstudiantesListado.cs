namespace Estudiantes_Matriculados.Entities
{
    public class EstudiantesListado
    {

        public int ID_Estudiante { get; set; }
        public string Tipo_Identificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre_Completo { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;


    }
}
