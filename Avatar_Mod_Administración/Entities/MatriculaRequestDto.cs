namespace Avatar_Mod_Administración.Entities
{
    public class MatriculaRequestDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public int ID_Curso { get; set; }
        public int ID_Grupo { get; set; }
        public int ID_Periodo { get; set; }


    }
}
