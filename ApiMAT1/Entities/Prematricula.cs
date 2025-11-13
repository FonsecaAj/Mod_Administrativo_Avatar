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
}
