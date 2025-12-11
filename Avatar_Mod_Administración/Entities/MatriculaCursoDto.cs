using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class MatriculaCursoDto
    {
        [JsonPropertyName("id_Curso")]
        public int ID_Curso { get; set; }

        [JsonPropertyName("nombre_Curso")]
        public string Nombre_Curso { get; set; } = string.Empty;

        [JsonPropertyName("codigo_Curso")]
        public string? Codigo_Curso { get; set; }

    }
}
