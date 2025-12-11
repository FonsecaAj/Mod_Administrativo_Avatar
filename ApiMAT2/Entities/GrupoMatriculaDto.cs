namespace ApiMAT2.Entities
{
    public class GrupoMatriculaDto
    {

        public int ID_Grupo { get; set; }
        public int ID_Curso { get; set; }
        public string? Nombre_Grupo { get; set; }

 
        public string? Nombre_Curso { get; set; }

        public int Cupo_Maximo { get; set; }
        public int Cupo_Disponible { get; set; }
    }
}
