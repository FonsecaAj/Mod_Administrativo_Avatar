using System.ComponentModel.DataAnnotations;

namespace ApiACD3.Entities
{
    public class Curso
    {
        [Key]
        public int ID_Curso { get; set; }            // Llave primaria
        public string Identificador { get; set; }   // Código del curso (INF101, etc.)

        public string Identificacion { get; set; }

        public string Codigo_Curso { get; set; }


        public int ID_Carrera { get; set; }          // FK hacia Carrera
        public int Nivel { get; set; }              // Nivel del curso (1-12)
        public string Nombre { get; set; }          // Solo letras y espacios
        
    }
}
