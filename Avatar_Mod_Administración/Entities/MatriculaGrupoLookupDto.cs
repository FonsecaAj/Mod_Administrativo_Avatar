using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class MatriculaGrupoLookupDto
    {
        [JsonPropertyName("ID_Grupo")]
        public int ID_Grupo { get; set; }

        [JsonPropertyName("ID_Curso")]
        public int ID_Curso { get; set; }

        [JsonPropertyName("Nombre_Curso")]
        public string Nombre_Curso { get; set; } = string.Empty;

        [JsonPropertyName("Nombre_Grupo")]
        public string Nombre_Grupo { get; set; } = string.Empty;

        [JsonPropertyName("Cupo_Disponible")]
        public int Cupo_Disponible { get; set; }

        [JsonPropertyName("EstadoGrupo")]
        public string EstadoGrupo { get; set; } = string.Empty;
    }
}
