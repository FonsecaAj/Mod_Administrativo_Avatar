using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class CursoDto
    {
        public int ID_Curso { get; set; }  
        public string Identificador { get; set; } = string.Empty;
        public int ID_Carrera { get; set; }
        public int Nivel { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }


}

