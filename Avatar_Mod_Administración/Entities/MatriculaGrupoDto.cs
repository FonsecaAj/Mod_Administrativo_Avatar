using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class MatriculaGrupoDto
    {

        [JsonPropertyName("id_Grupo")]
        public int ID_Grupo { get; set; }

        [JsonPropertyName("id_Curso")]
        public int ID_Curso { get; set; }

        [JsonPropertyName("nombre_Grupo")]
        public string? Nombre_Grupo { get; set; }

        [JsonPropertyName("nombre_Curso")]
        public string? Nombre_Curso { get; set; }

        [JsonPropertyName("cupo_Maximo")]
        public int Cupo_Maximo { get; set; }

        [JsonPropertyName("cupo_Disponible")]
        public int Cupo_Disponible { get; set; }

        [JsonIgnore]
        public string EstadoGrupo => Cupo_Disponible > 0 ? "Disponible" : "Lleno";
    }
}
