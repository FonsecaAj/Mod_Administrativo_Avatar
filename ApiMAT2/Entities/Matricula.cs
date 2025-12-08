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
}
