namespace ApiACD3
{
    public class MisCursoDto
    {
        public int ID_Curso { get; set; }
        public string Codigo_Curso { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        public string Grupo { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;

        public bool EsPeriodoActual { get; set; } = false;
    }
}
