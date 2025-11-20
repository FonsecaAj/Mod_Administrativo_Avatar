namespace Avatar_Mod_Administración.Entities
{
    public class PrematriculaDto
    {
        public int ID_Prematricula { get; set; }
        public int ID_Estudiante { get; set; }
        public int ID_Carrera { get; set; }
        public int ID_Curso { get; set; }
        public int ID_Periodo { get; set; }


        public string NombreEstudiante { get; set; } = "";
        public string NombreCarrera { get; set; } = "";
        public string NombreCurso { get; set; } = "";
        public string NombrePeriodo { get; set; } = "";

    }


}

