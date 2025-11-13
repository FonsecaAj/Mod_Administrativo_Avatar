namespace Mod_Academico.Entities
{
    public class HistorialAcademico
    {
        public string Codigo_Curso { get; set; } = string.Empty;
        public string Nombre_Curso { get; set; } = string.Empty;
        public decimal Promedio { get; set; }

    }

    public class HistorialRequest
    {
        public string Tipo_Identificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
    }


}
